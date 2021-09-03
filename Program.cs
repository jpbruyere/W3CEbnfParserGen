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
					return $"#x{Matche}{CardinalityString}";
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
W3CEbnfParserGen [options] input-file

	-o path		output file path.
	-h		this help message.
");
		}
        static void Main(string[] args)
        {
            int i = 0;
			string inputFile = null, outputFile = null;
			try
			{
				
				while (i < args.Length) {
					switch (args[i++]) {
						case "-h":
							printHelp ();
							return;
						case "-o":
							outputFile = args[i++];
							break;
						default:
							inputFile = args[i-1];
							break;
					}
				}
			} catch (System.Exception) {
				Console.WriteLine ($"Command error.");
				printHelp ();
				return;
			}
			if (string.IsNullOrEmpty (inputFile)) {
				Console.WriteLine ($"No ebnf input file specified.");
				printHelp ();
				return;
			}

			new Program (inputFile, outputFile).Generate ();
        }

		string inputPath, outputPath;
		
		public Program (string input, string output) {
			inputPath = input;
			if (string.IsNullOrEmpty (output))
				outputPath = $"{inputPath}.cs";
			else
				outputPath = output;
		}

		public void Generate () {
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
				ebnfToks.Where (tok => !tok.Type.HasFlag (TokenType.Trivia)).ToArray ());
			SymbolDecl[] symbols = ebnfSyntAnalyser.GetSymbols ();

			foreach (SymbolDecl sd in symbols) 
				Console.WriteLine ($"{sd}");

		}

    }
}
