﻿// Copyright (c) 2013-2021  Bruyère Jean-Philippe <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)

using System;

namespace W3CEbnfParserGen
{
	[Flags]
	public enum TokenType {
		Unknown,
		Trivia					= 0x0100,
		WhiteSpace				= 0x4100,
		Tabulation				= 0x4101,
		LineBreak				= 0x4102,
		EndOfFile				= 0x4103,
		LineComment				= 0x0103,
		BlockCommentStart		= 0x0104,
		BlockComment			= 0x0105,
		BlockCommentEnd			= 0x0106,
		Name					= 0x0200,
		SymbolName				= 0x0201,

		Punctuation				= 0x0400,
		OpenBracket				= 0x0401,
		ClosingBracket			= 0x0402,
		CharMatchOpen 			= 0x0403,// '['
		CharMatchClose			= 0x0404,// ']'
		StringMatchOpen			= 0x0405,
		StringMatchClose		= 0x0406,
		CharMatchNegation		= 0x0407,// '^'
		CharMatchRangeOperator	= 0x0C01,// '-'
		//CharMatch				= 0x0C01,// '-'

		Operator 				= 0x0800,
		SymbolAffectation 		= 0x0801,
		ChoiceOp		 		= 0x0802,
		ExclusionOp		 		= 0x0803,
		SequenceOp		 		= 0x0804, //never parsed (it's only a white space) but use in syntaxAnalyser
		CardinalityOp	 		= 0x0805,

		Match					= 0x2000,
		StringMatch				= 0x2001,
		CharMatch				= 0x2002,
		CodePointMatch			= 0x2003,
		TokenSectionStart,
	}
}