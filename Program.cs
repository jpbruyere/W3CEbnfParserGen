using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace W3CEbnfParserGen
{
	public class SymbolDecl {
		public readonly string Name;
		public Expression Expression;
		public SymbolDecl (string name) {
			Name = name;
		}
		public override string ToString() => $"{Name} ::= {Expression.ToString()}";
	}
	public abstract class Expression {
		public bool Optional, Single = true;
		public override string ToString() => Single ? Optional ? "?" : "" : Optional ? "*" : "+";
		public string CardinalityString => Single ? Optional ? "?" : "" : Optional ? "*" : "+";
	}
	public class TerminalExpression : Expression {
		public enum Type { Symbol, CharRange, String, CodePoint }
		public readonly Type ExpressionType;
		public readonly string Matche;
		public TerminalExpression (Type type, string matche) {
			ExpressionType = type;
			Matche = matche;
		}
		public override string ToString() {
			switch (ExpressionType)
			{
				case Type.Symbol:
					return $"{Matche}{CardinalityString}";
				case Type.CharRange:
					return $"[{Matche}]{CardinalityString}";
				case Type.CodePoint:
					return $"{Matche}{CardinalityString}";
				default:
					return $"'{Matche}'{CardinalityString}";
			}
		}
	}
	public class CompoundExpression : Expression {
		public enum Type { Exclusion = 2, Sequence = 3, Choice = 4 }
		public readonly Type CompExpType;
		public Expression FirstOperand;
		public Expression SecondOperand;
		public CompoundExpression (Type comoundExpressionType) {
			CompExpType = comoundExpressionType;
		}
		public bool HasCardinality => !(Single && !Optional);
		public int OpPrecedence => (int)CompExpType;
		public override string ToString()
		{
			StringBuilder tmp = new StringBuilder (32);
			if (HasCardinality)
				tmp.Append ("(");
			if (FirstOperand is CompoundExpression cmpExp) {
				if ((OpPrecedence < cmpExp.OpPrecedence) && !cmpExp.HasCardinality) 
					tmp.Append ($"({cmpExp}){cmpExp.CardinalityString}");
				else
					tmp.Append ($"{cmpExp}");
			} else
				tmp.Append ($"{FirstOperand}");

			switch (CompExpType)
			{
				case Type.Sequence:
					tmp.Append ($" ");
					break;
				case Type.Choice:
					tmp.Append ($" | ");
					break;
				default:
					tmp.Append ($" - ");
					break;
			}			
			if (SecondOperand is CompoundExpression cmpExp2) {
				if ((OpPrecedence < cmpExp2.OpPrecedence) && !cmpExp2.HasCardinality) 
					tmp.Append ($"({cmpExp2})");
				else
					tmp.Append ($"{cmpExp2}");
			} else
				tmp.Append ($"{SecondOperand}");
			
			if (HasCardinality)
				tmp.Append ($"){CardinalityString}");
			
			return tmp.ToString ();
		}
	}
	class EbnfParserException : Exception {
		public int Line, Column;
		public EbnfParserException (string message, int line = 0, int column = 0) : base (message) {
			Line = line;
			Column = column;
		}
		public override string ToString() => $"{base.ToString()} ({Line},{Column})";
	}
    class Program
    {
		static void printHelp () {
			Console.WriteLine (@"
W3C EBNF csharp parser generator.
W3CEbnfParserGen [options] input-files

	-o path		output file path.
	-h		this help message.
");
		}
        static void Main(string[] args)
        {
            int i = 0;
			List<string> inputFiles = new List<string> ();
			string outputFile = null;
			try
			{
				while (i < args.Length) {
					switch (args[i]) {
						case "-h":
							printHelp ();
							return;
						case "-o":
							i++;
							outputFile = args[i++];
							break;
						default:
							inputFiles.Add (args[i++]);
							break;
					}
				}
			} catch (System.Exception) {
				Console.WriteLine ($"Command error.");
				printHelp ();
				return;
			}
			if (inputFiles.Count == 0) {
				Console.WriteLine ($"No ebnf input file specified.");
				printHelp ();
				return;
			}

			new Program (outputFile, inputFiles.ToArray()).Generate ();
        }

		string[] inputFiles;
		string outputPath;

		
		public Program (string output, params string[] inputs) {
			inputFiles = inputs;
			if (string.IsNullOrEmpty (output)) {
				if (inputs.Length == 1)
					outputPath = $"{inputFiles[0]}_generated.cs";
				else
					outputPath = $"w3cEbnfParser_generated.cs";

			} else
				outputPath = output;
		}

		public void Generate () {
			List<SymbolDecl> symbols = new List<SymbolDecl> (100);
			foreach	(string inputFile in inputFiles)
				symbols.AddRange (parseEbnf (inputFile));
			
			
			foreach (SymbolDecl sd in symbols) 
				Console.WriteLine ($"{sd}");
		}
		SymbolDecl[] parseEbnf (string inputPath) {
			string ebnfSource = null;
			using (Stream s = new FileStream (inputPath, FileMode.Open)) {						
				using (StreamReader sr = new StreamReader (s)) 
					ebnfSource = sr.ReadToEnd ();
			}
			EbnfTokenizer ebnfTokenizer = new EbnfTokenizer ();
			Token[] ebnfToks = ebnfTokenizer.Tokenize (ebnfSource);

			foreach (Token tk in ebnfToks) {
				Console.WriteLine ($"{tk}\t\t{tk.AsString (ebnfSource)}");
			}

			EbnfSyntaxAnalyser ebnfSyntAnalyser = new EbnfSyntaxAnalyser (ebnfSource,
				removeSpuriousQuestionMarks (
				ebnfToks.Where (tok => !tok.Type.HasFlag (TokenType.Trivia)).ToArray ()));
			return ebnfSyntAnalyser.GetSymbols ();
		}

		Token[] removeSpuriousQuestionMarks (Token[] tokens) {
			if (tokens.Length == 0)
				return tokens;
			int i = 0;
			List<Token> toks = new List<Token> (tokens.Length);
			Token lastTok = tokens[i++];
			while (i < tokens.Length) {
				if (!(tokens[i].Type == TokenType.SymbolAffectation && lastTok.Type == TokenType.CardinalityOp))
					toks.Add (lastTok);
				lastTok = tokens[i++];
			}
			toks.Add (lastTok);
			return toks.ToArray ();
		}

    }
}
