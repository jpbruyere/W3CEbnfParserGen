// Copyright (c) 2013-2021  Bruyère Jean-Philippe <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;

namespace W3CEbnfParserGen
{
	public class EbnfTokenizer {
		int startOfTok;

		public EbnfTokenizer  () {}
		bool readName (ref SpanCharReader reader) {
			if (reader.EndOfSpan)
				return false;
			char c = reader.Peek;					
			if (char.IsLetter(c) || c == '_') {
				reader.Advance ();
				while (reader.TryPeek (ref c)) {									
					if (!(char.IsLetterOrDigit(c)|| c == '_'))
						return true;
					reader.Advance ();
				}
				return true;
			}
			return false;
		}
		bool readNumber (ref SpanCharReader reader) {
			if (reader.EndOfSpan)
				return false;		
			if (IsValidHexDigit (reader.Peek)) {
				reader.Advance ();
				while (IsValidHexDigit (reader.Peek))
					reader.Advance ();
				return true;
			}
			return false;
		}		
		protected List<Token> Toks;
		public static bool IsValidHexDigit (char c) =>
			char.IsDigit (c) || (c > 64 && c < 71) || (c > 96 && c < 103);

		void skipWhiteSpaces (ref SpanCharReader reader) {
			while(!reader.EndOfSpan) {
				switch (reader.Peek) {
					case '\x85':
					case '\x2028':
					case '\xA':
						reader.Read();
						addTok (ref reader, TokenType.LineBreak);
						break;
					case '\xD':
						reader.Read();
						if (reader.IsNextCharIn ('\xA', '\x85'))
							reader.Read();
						addTok (ref reader, TokenType.LineBreak);														
						break;
					case '\x20':
					case '\x9':
						char c = reader.Read();									
						while (reader.TryPeek (c))
							reader.Read();
						addTok (ref reader, c == '\x20' ? TokenType.WhiteSpace : TokenType.Tabulation);
						break;
					default:
						return;
				}
			}
		}
		void addTok (ref SpanCharReader reader, TokenType tokType) {
			if (reader.CurrentPosition == startOfTok)
				return;
			Toks.Add (new Token(tokType, startOfTok, reader.CurrentPosition));
			startOfTok = reader.CurrentPosition;
		}
		public Token[] Tokenize (string source) {
			SpanCharReader reader = new SpanCharReader(source);
			
			startOfTok = 0;
			Toks = new List<Token>(100);

			while(!reader.EndOfSpan) {

				skipWhiteSpaces (ref reader);

				if (reader.EndOfSpan)
					break;

				switch (reader.Peek) {				
				case '/':
					reader.Advance ();
					if (reader.TryRead ('*')) {
						addTok (ref reader, TokenType.BlockCommentStart);
						if (reader.TryReadUntil ("*/")) {
							addTok (ref reader, TokenType.BlockComment);
							reader.Advance (2);	
							addTok (ref reader, TokenType.BlockCommentEnd);
							break;
						}
					}
					throw new EbnfParserException ("malform comment");
				case '"':
				case '\'':
					char q = reader.Read();
					addTok (ref reader, TokenType.StringMatchOpen);
					if (reader.TryReadUntil (q)) {
						addTok (ref reader, TokenType.StringMatch);
						reader.Advance ();
						addTok (ref reader, TokenType.StringMatchClose);
					} else
						addTok (ref reader, TokenType.StringMatch);
					break;
				case ':':
					reader.Advance();
					if (!reader.TryRead (":="))
						throw new EbnfParserException ("malform symbol declaration, expecting '::='.");
					addTok (ref reader, TokenType.SymbolAffectation);
					break;
				case '<':
					reader.Advance();
					if (!reader.TryRead ("?TOKENS?>"))
						throw new EbnfParserException ("malform token section start, expecting '<?TOKENS?>'.");
					addTok (ref reader, TokenType.TokenSectionStart);
					break;
				case '(':
					reader.Advance();
					addTok (ref reader, TokenType.OpenBracket);
					break;
				case ')':
					reader.Advance();
					addTok (ref reader, TokenType.ClosingBracket);
					break;
				case '|':
					reader.Advance();
					addTok (ref reader, TokenType.ChoiceOp);
					break;
				case '-':
					reader.Advance();
					addTok (ref reader, TokenType.ExclusionOp);
					break;
				case '?':
				case '+':
				case '*':
					reader.Advance();
					addTok (ref reader, TokenType.CardinalityOp);
					break;
				case '[':
					reader.Advance();
					addTok (ref reader, TokenType.CharMatchOpen);
					if (reader.TryReadUntil ("]")) {
						addTok (ref reader, TokenType.CharMatch);
						reader.Advance (1);	
						addTok (ref reader, TokenType.CharMatchClose);
						break;
					}
					break;
				case '#':
					reader.Advance();
					if (reader.TryRead ('x')) {
						if (readNumber (ref reader)) {
							addTok (ref reader, TokenType.CodePointMatch);
							break;
						}
					}
					throw new EbnfParserException ("malform ucs character codepoint, expecting '#xN'.");
				default:
					if (readName(ref reader))
						addTok (ref reader, TokenType.SymbolName);
					else if (reader.TryAdvance())
						addTok (ref reader, TokenType.Unknown);
					break;
				}
			}

			return Toks.ToArray();
		}
		
	}
}
