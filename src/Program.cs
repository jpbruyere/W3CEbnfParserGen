// Copyright (c) 2021  Bruyère Jean-Philippe <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace W3CEbnfParserGen
{
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
			
			
			foreach (SymbolDecl sd in symbols) {
				Console.WriteLine ($"{sd.Name}");
				foreach (Expression e in sd.FlattenedExpressions.Where (ex => !(ex is TerminalExpression te && te.ExpressionType == TerminalExpression.Type.Symbol)))
					Console.WriteLine ($"\t{e}");
			}
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
