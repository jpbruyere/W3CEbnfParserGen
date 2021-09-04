// Copyright (c) 2013-2021  Bruyère Jean-Philippe <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;

namespace W3CEbnfParserGen
{
	public class EbnfSyntaxAnalyser {
		int curTokIndex;
		string source;
		Token[] tokens;
		Stack<object> resolveStack;//expression resolutions

		public EbnfSyntaxAnalyser  (string ebnfSource, Token[] ebnfTokens) {
			source = ebnfSource;
			tokens = ebnfTokens;
		}
		bool EOF => curTokIndex == tokens.Length;
		bool EndOfExpression =>
			EOF || curTokIndex > tokens.Length - 2 || tokens[curTokIndex + 1].Type == TokenType.SymbolAffectation;
		bool tryRead (out Token tok) {
			if (EOF) {
				tok = default;
				return false;
			}
			tok = tokens [curTokIndex++];
			return true;
		}
		bool tryPeek (out Token tok) {
			if (EOF) {
				tok = default;
				return false;
			}
			tok = tokens [curTokIndex];
			return true;
		}
		bool tryRead (out Token tok, TokenType expectedType) {
			if (EOF) {
				tok = default;
				return false;
			}
			tok = tokens [curTokIndex++];
			return tok.Type == expectedType;
		}		
		bool tryPeek (out Token tok, TokenType expectedType) {
			if (EOF) {
				tok = default;
				return false;
			}
			tok = tokens [curTokIndex];
			return tok.Type == expectedType;
		}
		bool resolvStackPeekIsOpenBracket =>
			resolveStack.TryPeek (out object elt) && elt is Token tok && tok.Type == TokenType.OpenBracket;
		bool resolvStackPeekIsSequenceOperator =>
			resolveStack.TryPeek (out object elt) && elt is Expression;

		Expression resolveStackPopExpression () {
			if (resolveStack.TryPeek (out object ro)) {
				if (ro is Expression exp) {
					resolveStack.Pop ();
					return exp;
				} else
					throw new EbnfParserException ($"resolve: expecting expression, having {ro}");
			} else
				throw new EbnfParserException ($"resolve: empty stack");
		}
		Expression resolveStackPeekExpression () {
			if (resolveStack.TryPeek (out object ro)) {
				if (ro is Expression exp)
					return exp;
				else
					throw new EbnfParserException ($"resolve: expecting expression, having {ro}");
			} else
				throw new EbnfParserException ($"resolve: empty stack");
		}
		bool resolveStackTryPeek<T> (out T obj) {
			if (resolveStack.TryPeek (out object ro)) {
				if (ro is T t) {
					obj = t;
					return true;
				}
			}
			obj = default;
			return false;
		}


		Expression resolve (Expression rightOp = null) {
			CompoundExpression compExp = default;
			if (rightOp == null)
				rightOp = resolveStackPopExpression ();	
			
			if (resolveStack.TryPeek (out object obj)) {
				if (obj is Token tok) {

					if (tok.Type == TokenType.OpenBracket)
						return rightOp;
					
					resolveStack.Pop ();
					if (tok.Type == TokenType.ChoiceOp)
						compExp = new CompoundExpression (CompoundExpression.Type.Choice) { SecondOperand = rightOp };
					else if (tok.Type == TokenType.ExclusionOp)
						compExp = new CompoundExpression (CompoundExpression.Type.Exclusion) { SecondOperand = rightOp };
					else
						throw new EbnfParserException ($"resolve: expecting operator, having {tok.Type}, {tok.AsString (source)}");

					if (resolveStack.TryPop (out obj) && obj is Expression exp) 
						compExp.FirstOperand = exp;
					else
						throw new EbnfParserException ($"resolve: expecting expression, having {obj}");
				} else if (obj is Expression exp) {
					resolveStack.Pop ();
					compExp = new CompoundExpression (CompoundExpression.Type.Sequence) {
						FirstOperand = exp,
						SecondOperand = rightOp
					};
				} else
					throw new EbnfParserException ($"resolve: unexpected stack element: {obj}");

				return compExp;
			}
			return rightOp;
		}
		Token Read ()  => tokens [curTokIndex++];
		Token Peek  => tokens [curTokIndex];

		int operatorPrecedance (Token tok) {
			if (tok.Type.HasFlag (TokenType.WhiteSpace))//sequence operator
				return 3;
			if (tok.Type == TokenType.ExclusionOp)
				return 2;
			if (tok.Type == TokenType.ChoiceOp)
				return 4;
			throw new EbnfParserException ($"unknow operator: {tok}");
		}

		void checkCardinalityAndPushNewExpression (Expression exp) {
			if (tryPeek (out Token tok, TokenType.CardinalityOp)) {
				Read ();
				switch (tok.AsString (source)) {
					case "?":
						exp.Optional = exp.Single = true;
						break;
					case "+":
						exp.Optional = exp.Single = false;
						break;
					case "*":
						exp.Optional = true;
						exp.Single = false;
						break;
				}
			}
			if (resolveStackTryPeek<Expression> (out Expression leftOp)) {//if exp on the stack, the implicit operator is sequence with precedence=3
				resolveStack.Pop ();
				if (resolveStackTryPeek<Token> (out tok)) {
					if (tok.Type.HasFlag (TokenType.Operator)) {
						if (operatorPrecedance (tok) <= 3)
							resolveStack.Push (resolve (leftOp));
						else
							resolveStack.Push (leftOp);
					} else if (tok.Type == TokenType.OpenBracket)
						resolveStack.Push (leftOp);
				} else //so theres an expression on the stack, the operator is sequenceOp (whitespace) with precedence = 3
					resolveStack.Push (resolve (leftOp));
			}
			resolveStack.Push (exp);
		}
		void storeSymbol () {
			Expression rightOp = resolve ();
			while (resolveStack.Count > 0)
				rightOp = resolve (rightOp);
			curSymbol.Expression = rightOp;
			symbols.Add (curSymbol);
			curSymbol = null;
			resolveStack = null;
		}
		List<SymbolDecl> symbols;
		SymbolDecl curSymbol;
		public SymbolDecl[] GetSymbols () {
			curTokIndex = 0;
			resolveStack = null;

			symbols = new List<SymbolDecl> (100);
			curSymbol = null;
			Token tok = default;

			while (!EOF) {
				if (Peek.Type == TokenType.TokenSectionStart) {
					Read ();
					continue;
				}

				if (resolveStack == null) {
					//no current symbol
					if (Peek.Type != TokenType.SymbolName)
						throw new EbnfParserException ($"expecing symbol name, having {Peek.Type}, {Peek.AsString (source)}");
					curSymbol = new SymbolDecl (Read ().AsString (source));
					if (!tryRead (out tok, TokenType.SymbolAffectation))
						throw new EbnfParserException ($"expecing '::='");
					resolveStack = new Stack<object> (16);
				} else if (Peek.Type == TokenType.OpenBracket) {
					tok = Read ();
					resolveStack.Push (tok);
				} else if (Peek.Type == TokenType.ClosingBracket) {
					tok = Read ();
					Expression rightOp = resolve ();
					while (!resolvStackPeekIsOpenBracket) 
						rightOp = resolve (rightOp);
					if (resolveStack.TryPop (out object obj) && obj is Token tk && tk.Type == TokenType.OpenBracket)
						checkCardinalityAndPushNewExpression (rightOp);
					else
						throw new EbnfParserException ($"expecing open bracket.");
				} else if (Peek.Type.HasFlag (TokenType.Punctuation)) {
					tok = Read ();
					Expression te = default;
					if (tok.Type == TokenType.CharMatchOpen) {
						if (tryRead (out tok, TokenType.CharMatch)) {
							te = new TerminalExpression (TerminalExpression.Type.CharRange, tok.AsString (source));
							if (!tryRead (out tok, TokenType.CharMatchClose))
								throw new EbnfParserException ($"malformed character range match");
						} else
							throw new EbnfParserException ($"malformed character range match");
					}
					if (tok.Type == TokenType.StringMatchOpen) {
						if (tryRead (out tok, TokenType.StringMatch)) {
							te = new TerminalExpression (TerminalExpression.Type.String, tok.AsString (source));
							if (!tryRead (out tok, TokenType.StringMatchClose))
								throw new EbnfParserException ($"malformed string match");
						} else
							throw new EbnfParserException ($"malformed string match");
					}
					checkCardinalityAndPushNewExpression (te);
				} else if (Peek.Type == TokenType.CodePointMatch) {
					tok = Read ();
					checkCardinalityAndPushNewExpression (new TerminalExpression (TerminalExpression.Type.CodePoint, tok.AsString (source)));
				} else if (Peek.Type == TokenType.SymbolName) {
					if (EndOfExpression) {
						storeSymbol ();
						continue;
					}
					tok = Read ();
					checkCardinalityAndPushNewExpression (new TerminalExpression (TerminalExpression.Type.Symbol, tok.AsString (source)));
				} else if (Peek.Type.HasFlag (TokenType.Operator)) {
					Token newOp = Read ();					
					if (newOp.Type == TokenType.SymbolAffectation || newOp.Type == TokenType.CardinalityOp)
						System.Diagnostics.Debugger.Break ();
					
					if (resolveStackTryPeek<Expression> (out Expression exp)) {
						if (resolveStack.Count > 1) {
							resolveStack.Pop ();
							if (resolveStackTryPeek<Token> (out tok)) {
								if (tok.Type.HasFlag (TokenType.Operator)) {
									if (operatorPrecedance (tok) <= operatorPrecedance (newOp))
										resolveStack.Push (resolve (exp));
									else
										resolveStack.Push (exp);
								} else if (tok.Type == TokenType.OpenBracket)
									resolveStack.Push (exp);
							} else if (3 <= operatorPrecedance (newOp)) { //so theres an expression on the stack, the operator is sequenceOp (whitespace) with precedence = 3
								resolveStack.Push (resolve (exp));
							} else
								resolveStack.Push (exp);
						}
						
						resolveStack.Push (newOp);
					} else
						System.Diagnostics.Debugger.Break ();
				} else
					System.Diagnostics.Debugger.Break ();
			}

			if (resolveStack != null && resolveStack.Count > 0) 
				storeSymbol ();

			return symbols.ToArray ();
		}		
		
	}
}
