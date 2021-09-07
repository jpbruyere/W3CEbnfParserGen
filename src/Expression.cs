// Copyright (c) 2021  Bruyère Jean-Philippe <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace W3CEbnfParserGen
{
	public abstract class Expression {
		public bool Optional, Single = true;
		public override string ToString() => Single ? Optional ? "?" : "" : Optional ? "*" : "+";
		public string CardinalityString => Single ? Optional ? "?" : "" : Optional ? "*" : "+";

		public virtual IEnumerable<Expression> Flatten {
			get {
				yield return this;
			}
		}
	}
	public class CharMatchExpression : Expression {
		public readonly bool Negative;
		public readonly CharRangeElement[] Elements;
		public CharMatchExpression (bool negative, params CharRangeElement[] elements) {
			Negative = negative;
			Elements = elements;
		}
		public override string ToString() => Elements.Select (e=>e.ToString()).Aggregate ((a,b) => $"{a}{b}");
	}
	public class SymbolMatch : Expression {
		public readonly string SymbolName;
		public SymbolMatch (string symbolName) {
			SymbolName = symbolName;
		}
		public override string ToString() => $"{SymbolName}{CardinalityString}";

	}
	public class StringMatch : Expression {
		public readonly string MatchString;
		public StringMatch (string matchString) {
			MatchString = matchString;
		}
		public override string ToString() => $"'{MatchString}'{CardinalityString}";

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
		public override IEnumerable<Expression> Flatten {
			get {
				foreach (Expression e in FirstOperand.Flatten)
					yield return e;
				foreach (Expression e in SecondOperand.Flatten)
					yield return e;
			}
		}

	}
}