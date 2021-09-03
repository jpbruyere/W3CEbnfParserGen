// This file was generated on Tue Aug 31, 2021 11:50 (UTC+02) by REx v5.53 which is Copyright (c) 1979-2021 by Gunther Rademacher <grd@gmx.net>
// REx command line: XML.ebnf -csharp

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class XML
{
  public class ParseException : Exception
  {
    private int begin, end, offending, expected, state;

    public ParseException(int b, int e, int s, int o, int x)
    {
      begin = b;
      end = e;
      state = s;
      offending = o;
      expected = x;
    }

    public String getMessage()
    {
      return offending < 0
           ? "lexical analysis failed"
           : "syntax error";
    }

    public int getBegin() {return begin;}
    public int getEnd() {return end;}
    public int getState() {return state;}
    public int getOffending() {return offending;}
    public int getExpected() {return expected;}
    public bool isAmbiguousInput() {return false;}
  }

  public XML(String s)
  {
    initialize(s);
  }

  public void initialize(String source)
  {
    input = source;
    size = source.Length;
    reset(0, 0, 0);
  }

  public String getInput()
  {
    return input;
  }

  public int getTokenOffset()
  {
    return b0;
  }

  public int getTokenEnd()
  {
    return e0;
  }

  public void reset(int l, int b, int e)
  {
            b0 = b; e0 = b;
    l1 = l; b1 = b; e1 = e;
    l2 = 0; b2 = 0; e2 = 0;
    l3 = 0; b3 = 0; e3 = 0;
    end = e;
  }

  public void reset()
  {
    reset(0, 0, 0);
  }

  public static String getOffendingToken(ParseException e)
  {
    return e.getOffending() < 0 ? null : TOKEN[e.getOffending()];
  }

  public static String[] getExpectedTokenSet(ParseException e)
  {
    String[] expected;
    if (e.getExpected() < 0)
    {
      expected = getTokenSet(- e.getState());
    }
    else
    {
      expected = new String[]{TOKEN[e.getExpected()]};
    }
    return expected;
  }

  public String getErrorMessage(ParseException e)
  {
    String message = e.getMessage();
    String[] tokenSet = getExpectedTokenSet(e);
    String found = getOffendingToken(e);
    int size = e.getEnd() - e.getBegin();
    message += (found == null ? "" : ", found " + found)
            + "\nwhile expecting "
            + (tokenSet.Length == 1 ? tokenSet[0] : ("[" + String.Join(", ", tokenSet) + "]"))
            + "\n"
            + (size == 0 || found != null ? "" : "after successfully scanning " + size + " characters beginning ");
    String prefix = input.Substring(0, e.getBegin());
    int line = prefix.Length - prefix.Replace("\n", "").Length + 1;
    int column = prefix.Length - prefix.LastIndexOf('\n');
    return message
         + "at line " + line + ", column " + column + ":\n..."
         + input.Substring(e.getBegin(), Math.Min(input.Length, e.getBegin() + 64) - e.getBegin())
         + "...";
  }

  public void parse_document()
  {
    parse_prolog();
    parse_element();
    for (;;)
    {
      lookahead1(56);               // EOF | S | Comment | PI
      if (l1 == 1)                  // EOF
      {
        break;
      }
      parse_Misc();
    }
    consume(1);                     // EOF
  }

  private void parse_prolog()
  {
    lookahead1(66);                 // S | Comment | PI | '<' | '<!DOCTYPE' | '<?xml'
    if (l1 == 40)                   // '<?xml'
    {
      parse_XMLDecl();
    }
    for (;;)
    {
      lookahead1(64);               // S | Comment | PI | '<' | '<!DOCTYPE'
      if (l1 == 33                  // '<'
       || l1 == 35)                 // '<!DOCTYPE'
      {
        break;
      }
      parse_Misc();
    }
    if (l1 == 35)                   // '<!DOCTYPE'
    {
      parse_doctypedecl();
      for (;;)
      {
        lookahead1(59);             // S | Comment | PI | '<'
        if (l1 == 33)               // '<'
        {
          break;
        }
        parse_Misc();
      }
    }
  }

  private void parse_XMLDecl()
  {
    consume(40);                    // '<?xml'
    parse_VersionInfo();
    lookahead1(28);                 // S | '?>'
    switch (l1)
    {
    case 2:                         // S
      lookahead2(55);               // '?>' | 'encoding' | 'standalone'
      break;
    default:
      lk = l1;
      break;
    }
    if (lk == 7810)                 // S 'encoding'
    {
      parse_EncodingDecl();
    }
    lookahead1(28);                 // S | '?>'
    switch (l1)
    {
    case 2:                         // S
      lookahead2(40);               // '?>' | 'standalone'
      break;
    default:
      lk = l1;
      break;
    }
    if (lk == 8066)                 // S 'standalone'
    {
      parse_SDDecl();
    }
    lookahead1(28);                 // S | '?>'
    if (l1 == 2)                    // S
    {
      consume(2);                   // S
    }
    lookahead1(17);                 // '?>'
    consume(44);                    // '?>'
  }

  private void parse_VersionInfo()
  {
    lookahead1(0);                  // S
    consume(2);                     // S
    lookahead1(21);                 // 'version'
    consume(64);                    // 'version'
    parse_Eq();
    lookahead1(33);                 // '"' | "'"
    switch (l1)
    {
    case 24:                        // "'"
      consume(24);                  // "'"
      lookahead1(6);                // VersionNum
      consume(13);                  // VersionNum
      lookahead1(11);               // "'"
      consume(24);                  // "'"
      break;
    default:
      consume(17);                  // '"'
      lookahead1(6);                // VersionNum
      consume(13);                  // VersionNum
      lookahead1(8);                // '"'
      consume(17);                  // '"'
      break;
    }
  }

  private void parse_Eq()
  {
    lookahead1(26);                 // S | '='
    if (l1 == 2)                    // S
    {
      consume(2);                   // S
    }
    lookahead1(15);                 // '='
    consume(41);                    // '='
    lookahead1(58);                 // S | AttValue | '"' | "'"
    if (l1 == 2)                    // S
    {
      consume(2);                   // S
    }
  }

  private void parse_Misc()
  {
    switch (l1)
    {
    case 10:                        // Comment
      consume(10);                  // Comment
      break;
    case 11:                        // PI
      consume(11);                  // PI
      break;
    default:
      consume(2);                   // S
      break;
    }
  }

  private void parse_doctypedecl()
  {
    consume(35);                    // '<!DOCTYPE'
    lookahead1(0);                  // S
    consume(2);                     // S
    lookahead1(1);                  // Name
    consume(3);                     // Name
    lookahead1(48);                 // S | '>' | '['
    switch (l1)
    {
    case 2:                         // S
      lookahead2(63);               // '>' | 'PUBLIC' | 'SYSTEM' | '['
      break;
    default:
      lk = l1;
      break;
    }
    if (lk == 7298                  // S 'PUBLIC'
     || lk == 7426)                 // S 'SYSTEM'
    {
      consume(2);                   // S
      parse_ExternalID();
    }
    lookahead1(48);                 // S | '>' | '['
    if (l1 == 2)                    // S
    {
      consume(2);                   // S
    }
    lookahead1(39);                 // '>' | '['
    if (l1 == 59)                   // '['
    {
      consume(59);                  // '['
      parse_intSubset();
      consume(60);                  // ']'
      lookahead1(27);               // S | '>'
      if (l1 == 2)                  // S
      {
        consume(2);                 // S
      }
    }
    lookahead1(16);                 // '>'
    consume(42);                    // '>'
  }

  private void parse_DeclSep()
  {
    switch (l1)
    {
    case 15:                        // PEReference
      consume(15);                  // PEReference
      break;
    default:
      consume(2);                   // S
      break;
    }
  }

  private void parse_intSubset()
  {
    for (;;)
    {
      lookahead1(70);               // S | Comment | PI | PEReference | '<!ATTLIST' | '<!ELEMENT' | '<!ENTITY' |
                                    // '<!NOTATION' | ']'
      if (l1 == 60)                 // ']'
      {
        break;
      }
      switch (l1)
      {
      case 2:                       // S
      case 15:                      // PEReference
        parse_DeclSep();
        break;
      default:
        parse_markupdecl();
        break;
      }
    }
  }

  private void parse_markupdecl()
  {
    switch (l1)
    {
    case 36:                        // '<!ELEMENT'
      parse_elementdecl();
      break;
    case 34:                        // '<!ATTLIST'
      parse_AttlistDecl();
      break;
    case 37:                        // '<!ENTITY'
      parse_EntityDecl();
      break;
    case 38:                        // '<!NOTATION'
      parse_NotationDecl();
      break;
    case 11:                        // PI
      consume(11);                  // PI
      break;
    default:
      consume(10);                  // Comment
      break;
    }
  }

  private void parse_SDDecl()
  {
    consume(2);                     // S
    lookahead1(20);                 // 'standalone'
    consume(63);                    // 'standalone'
    parse_Eq();
    lookahead1(33);                 // '"' | "'"
    switch (l1)
    {
    case 24:                        // "'"
      consume(24);                  // "'"
      lookahead1(42);               // 'no' | 'yes'
      switch (l1)
      {
      case 65:                      // 'yes'
        consume(65);                // 'yes'
        break;
      default:
        consume(62);                // 'no'
        break;
      }
      lookahead1(11);               // "'"
      consume(24);                  // "'"
      break;
    default:
      consume(17);                  // '"'
      lookahead1(42);               // 'no' | 'yes'
      switch (l1)
      {
      case 65:                      // 'yes'
        consume(65);                // 'yes'
        break;
      default:
        consume(62);                // 'no'
        break;
      }
      lookahead1(8);                // '"'
      consume(17);                  // '"'
      break;
    }
  }

  private void parse_element()
  {
    consume(33);                    // '<'
    lookahead1(1);                  // Name
    consume(3);                     // Name
    for (;;)
    {
      lookahead1(47);               // S | '/>' | '>'
      switch (l1)
      {
      case 2:                       // S
        lookahead2(50);             // Name | '/>' | '>'
        break;
      default:
        lk = l1;
        break;
      }
      if (lk != 386)                // S Name
      {
        break;
      }
      consume(2);                   // S
      parse_Attribute();
    }
    if (l1 == 2)                    // S
    {
      consume(2);                   // S
    }
    lookahead1(37);                 // '/>' | '>'
    switch (l1)
    {
    case 31:                        // '/>'
      consume(31);                  // '/>'
      break;
    default:
      consume(42);                  // '>'
      parse_content();
      parse_ETag();
      break;
    }
  }

  private void parse_Attribute()
  {
    lookahead1(1);                  // Name
    consume(3);                     // Name
    parse_Eq();
    lookahead1(3);                  // AttValue
    consume(6);                     // AttValue
  }

  private void parse_ETag()
  {
    consume(39);                    // '</'
    lookahead1(1);                  // Name
    consume(3);                     // Name
    lookahead1(27);                 // S | '>'
    if (l1 == 2)                    // S
    {
      consume(2);                   // S
    }
    lookahead1(16);                 // '>'
    consume(42);                    // '>'
  }

  private void parse_content()
  {
    lookahead1(69);                 // CharData | Comment | PI | CDSect | CharRef | '&' | '<' | '</'
    if (l1 == 9)                    // CharData
    {
      consume(9);                   // CharData
    }
    for (;;)
    {
      lookahead1(68);               // Comment | PI | CDSect | CharRef | '&' | '<' | '</'
      if (l1 == 39)                 // '</'
      {
        break;
      }
      switch (l1)
      {
      case 33:                      // '<'
        parse_element();
        break;
      case 12:                      // CDSect
        consume(12);                // CDSect
        break;
      case 11:                      // PI
        consume(11);                // PI
        break;
      case 10:                      // Comment
        consume(10);                // Comment
        break;
      default:
        parse_Reference();
        break;
      }
      lookahead1(69);               // CharData | Comment | PI | CDSect | CharRef | '&' | '<' | '</'
      if (l1 == 9)                  // CharData
      {
        consume(9);                 // CharData
      }
    }
  }

  private void parse_elementdecl()
  {
    consume(36);                    // '<!ELEMENT'
    lookahead1(0);                  // S
    consume(2);                     // S
    lookahead1(1);                  // Name
    consume(3);                     // Name
    lookahead1(0);                  // S
    consume(2);                     // S
    parse_contentspec();
    lookahead1(27);                 // S | '>'
    if (l1 == 2)                    // S
    {
      consume(2);                   // S
    }
    lookahead1(16);                 // '>'
    consume(42);                    // '>'
  }

  private void parse_contentspec()
  {
    lookahead1(52);                 // '(' | 'ANY' | 'EMPTY'
    switch (l1)
    {
    case 25:                        // '('
      lookahead2(57);               // S | Name | '#PCDATA' | '('
      switch (lk)
      {
      case 281:                     // '(' S
        lookahead3(49);             // Name | '#PCDATA' | '('
        break;
      }
      break;
    default:
      lk = l1;
      break;
    }
    switch (lk)
    {
    case 47:                        // 'EMPTY'
      consume(47);                  // 'EMPTY'
      break;
    case 45:                        // 'ANY'
      consume(45);                  // 'ANY'
      break;
    case 2585:                      // '(' '#PCDATA'
    case 327961:                    // '(' S '#PCDATA'
      parse_Mixed();
      break;
    default:
      parse_children();
      break;
    }
  }

  private void parse_children()
  {
    parse_choiceOrSeq();
    lookahead1(65);                 // S | '*' | '+' | '>' | '?'
    if (l1 != 2                     // S
     && l1 != 42)                   // '>'
    {
      switch (l1)
      {
      case 43:                      // '?'
        consume(43);                // '?'
        break;
      case 28:                      // '*'
        consume(28);                // '*'
        break;
      default:
        consume(29);                // '+'
        break;
      }
    }
  }

  private void parse_cp()
  {
    lookahead1(30);                 // Name | '('
    switch (l1)
    {
    case 3:                         // Name
      consume(3);                   // Name
      break;
    default:
      parse_choiceOrSeq();
      break;
    }
    lookahead1(67);                 // S | ')' | '*' | '+' | ',' | '?' | '|'
    if (l1 == 28                    // '*'
     || l1 == 29                    // '+'
     || l1 == 43)                   // '?'
    {
      switch (l1)
      {
      case 43:                      // '?'
        consume(43);                // '?'
        break;
      case 28:                      // '*'
        consume(28);                // '*'
        break;
      default:
        consume(29);                // '+'
        break;
      }
    }
  }

  private void parse_choiceOrSeq()
  {
    consume(25);                    // '('
    lookahead1(43);                 // S | Name | '('
    if (l1 == 2)                    // S
    {
      consume(2);                   // S
    }
    parse_cp();
    lookahead1(61);                 // S | ')' | ',' | '|'
    if (l1 == 2)                    // S
    {
      consume(2);                   // S
    }
    lookahead1(54);                 // ')' | ',' | '|'
    switch (l1)
    {
    case 66:                        // '|'
      for (;;)
      {
        consume(66);                // '|'
        lookahead1(43);             // S | Name | '('
        if (l1 == 2)                // S
        {
          consume(2);               // S
        }
        parse_cp();
        lookahead1(45);             // S | ')' | '|'
        if (l1 == 2)                // S
        {
          consume(2);               // S
        }
        lookahead1(35);             // ')' | '|'
        if (l1 != 66)               // '|'
        {
          break;
        }
      }
      break;
    default:
      for (;;)
      {
        lookahead1(34);             // ')' | ','
        if (l1 != 30)               // ','
        {
          break;
        }
        consume(30);                // ','
        lookahead1(43);             // S | Name | '('
        if (l1 == 2)                // S
        {
          consume(2);               // S
        }
        parse_cp();
        lookahead1(44);             // S | ')' | ','
        if (l1 == 2)                // S
        {
          consume(2);               // S
        }
      }
      break;
    }
    consume(26);                    // ')'
  }

  private void parse_Mixed()
  {
    consume(25);                    // '('
    lookahead1(25);                 // S | '#PCDATA'
    if (l1 == 2)                    // S
    {
      consume(2);                   // S
    }
    lookahead1(9);                  // '#PCDATA'
    consume(20);                    // '#PCDATA'
    lookahead1(60);                 // S | ')' | ')*' | '|'
    if (l1 == 2)                    // S
    {
      consume(2);                   // S
    }
    lookahead1(53);                 // ')' | ')*' | '|'
    switch (l1)
    {
    case 26:                        // ')'
      consume(26);                  // ')'
      break;
    default:
      for (;;)
      {
        lookahead1(36);             // ')*' | '|'
        if (l1 != 66)               // '|'
        {
          break;
        }
        consume(66);                // '|'
        lookahead1(23);             // S | Name
        if (l1 == 2)                // S
        {
          consume(2);               // S
        }
        lookahead1(1);              // Name
        consume(3);                 // Name
        lookahead1(46);             // S | ')*' | '|'
        if (l1 == 2)                // S
        {
          consume(2);               // S
        }
      }
      consume(27);                  // ')*'
      break;
    }
  }

  private void parse_AttlistDecl()
  {
    consume(34);                    // '<!ATTLIST'
    lookahead1(0);                  // S
    consume(2);                     // S
    lookahead1(1);                  // Name
    consume(3);                     // Name
    for (;;)
    {
      lookahead1(27);               // S | '>'
      switch (l1)
      {
      case 2:                       // S
        lookahead2(31);             // Name | '>'
        break;
      default:
        lk = l1;
        break;
      }
      if (lk != 386)                // S Name
      {
        break;
      }
      parse_AttDef();
    }
    if (l1 == 2)                    // S
    {
      consume(2);                   // S
    }
    lookahead1(16);                 // '>'
    consume(42);                    // '>'
  }

  private void parse_AttDef()
  {
    consume(2);                     // S
    lookahead1(1);                  // Name
    consume(3);                     // Name
    lookahead1(0);                  // S
    consume(2);                     // S
    parse_AttType();
    lookahead1(0);                  // S
    consume(2);                     // S
    parse_DefaultDecl();
  }

  private void parse_AttType()
  {
    lookahead1(71);                 // '(' | 'CDATA' | 'ENTITIES' | 'ENTITY' | 'ID' | 'IDREF' | 'IDREFS' | 'NMTOKEN' |
                                    // 'NMTOKENS' | 'NOTATION'
    switch (l1)
    {
    case 46:                        // 'CDATA'
      parse_StringType();
      break;
    case 25:                        // '('
    case 56:                        // 'NOTATION'
      parse_EnumeratedType();
      break;
    default:
      parse_TokenizedType();
      break;
    }
  }

  private void parse_StringType()
  {
    consume(46);                    // 'CDATA'
  }

  private void parse_TokenizedType()
  {
    switch (l1)
    {
    case 50:                        // 'ID'
      consume(50);                  // 'ID'
      break;
    case 51:                        // 'IDREF'
      consume(51);                  // 'IDREF'
      break;
    case 52:                        // 'IDREFS'
      consume(52);                  // 'IDREFS'
      break;
    case 49:                        // 'ENTITY'
      consume(49);                  // 'ENTITY'
      break;
    case 48:                        // 'ENTITIES'
      consume(48);                  // 'ENTITIES'
      break;
    case 54:                        // 'NMTOKEN'
      consume(54);                  // 'NMTOKEN'
      break;
    default:
      consume(55);                  // 'NMTOKENS'
      break;
    }
  }

  private void parse_EnumeratedType()
  {
    switch (l1)
    {
    case 56:                        // 'NOTATION'
      parse_NotationType();
      break;
    default:
      parse_Enumeration();
      break;
    }
  }

  private void parse_NotationType()
  {
    consume(56);                    // 'NOTATION'
    lookahead1(0);                  // S
    consume(2);                     // S
    lookahead1(12);                 // '('
    consume(25);                    // '('
    lookahead1(23);                 // S | Name
    if (l1 == 2)                    // S
    {
      consume(2);                   // S
    }
    lookahead1(1);                  // Name
    consume(3);                     // Name
    for (;;)
    {
      lookahead1(45);               // S | ')' | '|'
      switch (l1)
      {
      case 2:                       // S
        lookahead2(35);             // ')' | '|'
        break;
      default:
        lk = l1;
        break;
      }
      if (lk != 66                  // '|'
       && lk != 8450)               // S '|'
      {
        break;
      }
      if (l1 == 2)                  // S
      {
        consume(2);                 // S
      }
      lookahead1(22);               // '|'
      consume(66);                  // '|'
      lookahead1(23);               // S | Name
      if (l1 == 2)                  // S
      {
        consume(2);                 // S
      }
      lookahead1(1);                // Name
      consume(3);                   // Name
    }
    if (l1 == 2)                    // S
    {
      consume(2);                   // S
    }
    lookahead1(13);                 // ')'
    consume(26);                    // ')'
  }

  private void parse_Enumeration()
  {
    consume(25);                    // '('
    lookahead1(24);                 // S | Nmtoken
    if (l1 == 2)                    // S
    {
      consume(2);                   // S
    }
    lookahead1(2);                  // Nmtoken
    consume(4);                     // Nmtoken
    for (;;)
    {
      lookahead1(45);               // S | ')' | '|'
      switch (l1)
      {
      case 2:                       // S
        lookahead2(35);             // ')' | '|'
        break;
      default:
        lk = l1;
        break;
      }
      if (lk != 66                  // '|'
       && lk != 8450)               // S '|'
      {
        break;
      }
      if (l1 == 2)                  // S
      {
        consume(2);                 // S
      }
      lookahead1(22);               // '|'
      consume(66);                  // '|'
      lookahead1(24);               // S | Nmtoken
      if (l1 == 2)                  // S
      {
        consume(2);                 // S
      }
      lookahead1(2);                // Nmtoken
      consume(4);                   // Nmtoken
    }
    if (l1 == 2)                    // S
    {
      consume(2);                   // S
    }
    lookahead1(13);                 // ')'
    consume(26);                    // ')'
  }

  private void parse_DefaultDecl()
  {
    lookahead1(62);                 // AttValue | '#FIXED' | '#IMPLIED' | '#REQUIRED'
    switch (l1)
    {
    case 21:                        // '#REQUIRED'
      consume(21);                  // '#REQUIRED'
      break;
    case 19:                        // '#IMPLIED'
      consume(19);                  // '#IMPLIED'
      break;
    default:
      if (l1 == 18)                 // '#FIXED'
      {
        consume(18);                // '#FIXED'
        lookahead1(0);              // S
        consume(2);                 // S
      }
      lookahead1(3);                // AttValue
      consume(6);                   // AttValue
      break;
    }
  }

  private void parse_Reference()
  {
    switch (l1)
    {
    case 23:                        // '&'
      parse_EntityRef();
      break;
    default:
      consume(14);                  // CharRef
      break;
    }
  }

  private void parse_EntityRef()
  {
    consume(23);                    // '&'
    lookahead1(1);                  // Name
    consume(3);                     // Name
    lookahead1(14);                 // ';'
    consume(32);                    // ';'
  }

  private void parse_EntityDecl()
  {
    switch (l1)
    {
    case 37:                        // '<!ENTITY'
      lookahead2(0);                // S
      switch (lk)
      {
      case 293:                     // '<!ENTITY' S
        lookahead3(29);             // Name | '%'
        break;
      }
      break;
    default:
      lk = l1;
      break;
    }
    switch (lk)
    {
    case 49445:                     // '<!ENTITY' S Name
      parse_GEDecl();
      break;
    default:
      parse_PEDecl();
      break;
    }
  }

  private void parse_GEDecl()
  {
    consume(37);                    // '<!ENTITY'
    lookahead1(0);                  // S
    consume(2);                     // S
    lookahead1(1);                  // Name
    consume(3);                     // Name
    lookahead1(0);                  // S
    consume(2);                     // S
    parse_EntityDef();
    lookahead1(27);                 // S | '>'
    if (l1 == 2)                    // S
    {
      consume(2);                   // S
    }
    lookahead1(16);                 // '>'
    consume(42);                    // '>'
  }

  private void parse_PEDecl()
  {
    consume(37);                    // '<!ENTITY'
    lookahead1(0);                  // S
    consume(2);                     // S
    lookahead1(10);                 // '%'
    consume(22);                    // '%'
    lookahead1(0);                  // S
    consume(2);                     // S
    lookahead1(1);                  // Name
    consume(3);                     // Name
    lookahead1(0);                  // S
    consume(2);                     // S
    parse_PEDef();
    lookahead1(27);                 // S | '>'
    if (l1 == 2)                    // S
    {
      consume(2);                   // S
    }
    lookahead1(16);                 // '>'
    consume(42);                    // '>'
  }

  private void parse_EntityDef()
  {
    lookahead1(51);                 // EntityValue | 'PUBLIC' | 'SYSTEM'
    switch (l1)
    {
    case 5:                         // EntityValue
      consume(5);                   // EntityValue
      break;
    default:
      parse_ExternalID();
      lookahead1(27);               // S | '>'
      switch (l1)
      {
      case 2:                       // S
        lookahead2(38);             // '>' | 'NDATA'
        break;
      default:
        lk = l1;
        break;
      }
      if (lk == 6786)               // S 'NDATA'
      {
        parse_NDataDecl();
      }
      break;
    }
  }

  private void parse_PEDef()
  {
    lookahead1(51);                 // EntityValue | 'PUBLIC' | 'SYSTEM'
    switch (l1)
    {
    case 5:                         // EntityValue
      consume(5);                   // EntityValue
      break;
    default:
      parse_ExternalID();
      break;
    }
  }

  private void parse_ExternalID()
  {
    lookahead1(41);                 // 'PUBLIC' | 'SYSTEM'
    switch (l1)
    {
    case 58:                        // 'SYSTEM'
      consume(58);                  // 'SYSTEM'
      lookahead1(0);                // S
      consume(2);                   // S
      lookahead1(4);                // SystemLiteral
      consume(7);                   // SystemLiteral
      break;
    default:
      consume(57);                  // 'PUBLIC'
      lookahead1(0);                // S
      consume(2);                   // S
      lookahead1(5);                // PubidLiteral
      consume(8);                   // PubidLiteral
      lookahead1(0);                // S
      consume(2);                   // S
      lookahead1(4);                // SystemLiteral
      consume(7);                   // SystemLiteral
      break;
    }
  }

  private void parse_NDataDecl()
  {
    consume(2);                     // S
    lookahead1(18);                 // 'NDATA'
    consume(53);                    // 'NDATA'
    lookahead1(0);                  // S
    consume(2);                     // S
    lookahead1(1);                  // Name
    consume(3);                     // Name
  }

  private void parse_EncodingDecl()
  {
    consume(2);                     // S
    lookahead1(19);                 // 'encoding'
    consume(61);                    // 'encoding'
    parse_Eq();
    lookahead1(33);                 // '"' | "'"
    switch (l1)
    {
    case 17:                        // '"'
      consume(17);                  // '"'
      lookahead1(7);                // EncName
      consume(16);                  // EncName
      lookahead1(8);                // '"'
      consume(17);                  // '"'
      break;
    default:
      consume(24);                  // "'"
      lookahead1(7);                // EncName
      consume(16);                  // EncName
      lookahead1(11);               // "'"
      consume(24);                  // "'"
      break;
    }
  }

  private void parse_NotationDecl()
  {
    consume(38);                    // '<!NOTATION'
    lookahead1(0);                  // S
    consume(2);                     // S
    lookahead1(1);                  // Name
    consume(3);                     // Name
    lookahead1(0);                  // S
    consume(2);                     // S
    parse_ExternalOrPublicID();
    lookahead1(27);                 // S | '>'
    if (l1 == 2)                    // S
    {
      consume(2);                   // S
    }
    lookahead1(16);                 // '>'
    consume(42);                    // '>'
  }

  private void parse_ExternalOrPublicID()
  {
    lookahead1(41);                 // 'PUBLIC' | 'SYSTEM'
    switch (l1)
    {
    case 58:                        // 'SYSTEM'
      consume(58);                  // 'SYSTEM'
      lookahead1(0);                // S
      consume(2);                   // S
      lookahead1(4);                // SystemLiteral
      consume(7);                   // SystemLiteral
      break;
    default:
      consume(57);                  // 'PUBLIC'
      lookahead1(0);                // S
      consume(2);                   // S
      lookahead1(5);                // PubidLiteral
      consume(8);                   // PubidLiteral
      lookahead1(27);               // S | '>'
      switch (l1)
      {
      case 2:                       // S
        lookahead2(32);             // SystemLiteral | '>'
        break;
      default:
        lk = l1;
        break;
      }
      if (lk == 898)                // S SystemLiteral
      {
        consume(2);                 // S
        lookahead1(4);              // SystemLiteral
        consume(7);                 // SystemLiteral
      }
      break;
    }
  }

  private void consume(int t)
  {
    if (l1 == t)
    {
      b0 = b1; e0 = e1; l1 = l2; if (l1 != 0) {
      b1 = b2; e1 = e2; l2 = l3; if (l2 != 0) {
      b2 = b3; e2 = e3; l3 = 0; }}
    }
    else
    {
      error(b1, e1, 0, l1, t);
    }
  }

  private void lookahead1(int tokenSetId)
  {
    if (l1 == 0)
    {
      l1 = match(tokenSetId);
      b1 = begin;
      e1 = end;
    }
  }

  private void lookahead2(int tokenSetId)
  {
    if (l2 == 0)
    {
      l2 = match(tokenSetId);
      b2 = begin;
      e2 = end;
    }
    lk = (l2 << 7) | l1;
  }

  private void lookahead3(int tokenSetId)
  {
    if (l3 == 0)
    {
      l3 = match(tokenSetId);
      b3 = begin;
      e3 = end;
    }
    lk |= l3 << 14;
  }

  private int error(int b, int e, int s, int l, int t)
  {
    throw new ParseException(b, e, s, l, t);
  }

  private int lk, b0, e0;
  private int l1, b1, e1;
  private int l2, b2, e2;
  private int l3, b3, e3;
  private String input = null;
  private int size = 0;
  private int begin = 0;
  private int end = 0;

  private int match(int tokenSetId)
  {
    begin = end;
    int current = end;
    int result = INITIAL[tokenSetId];
    int state = 0;

    for (int code = result & 511; code != 0; )
    {
      int charclass;
      int c0 = current < size ? input[current] : 0;
      ++current;
      if (c0 < 0x80)
      {
        charclass = MAP0[c0];
      }
      else if (c0 < 0xd800)
      {
        int c1 = c0 >> 4;
        charclass = MAP1[(c0 & 15) + MAP1[(c1 & 31) + MAP1[c1 >> 5]]];
      }
      else
      {
        if (c0 < 0xdc00)
        {
          int c1 = current < size ? input[current] : 0;
          if (c1 >= 0xdc00 && c1 < 0xe000)
          {
            ++current;
            c0 = ((c0 & 0x3ff) << 10) + (c1 & 0x3ff) + 0x10000;
          }
        }

        int lo = 0, hi = 4;
        for (int m = 2; ; m = (hi + lo) >> 1)
        {
          if (MAP2[m] > c0) {hi = m - 1;}
          else if (MAP2[5 + m] < c0) {lo = m + 1;}
          else {charclass = MAP2[10 + m]; break;}
          if (lo > hi) {charclass = 0; break;}
        }
      }

      state = code;
      int i0 = (charclass << 9) + code - 1;
      code = TRANSITION[(i0 & 15) + TRANSITION[i0 >> 4]];

      if (code > 511)
      {
        result = code;
        code &= 511;
        end = current;
      }
    }

    result >>= 9;
    if (result == 0)
    {
      end = current - 1;
      int c1 = end < size ? input[end] : 0;
      if (c1 >= 0xdc00 && c1 < 0xe000)
      {
        --end;
      }
      return error(begin, end, state, -1, -1);
    }

    if (end > size) end = size;
    return (result & 127) - 1;
  }

  private static String[] getTokenSet(int tokenSetId)
  {
    List<String> expected = new List<String>();
    int s = tokenSetId < 0 ? - tokenSetId : INITIAL[tokenSetId] & 511;
    for (int i = 0; i < 67; i += 32)
    {
      int j = i;
      int i0 = (i >> 5) * 280 + s - 1;
      int i1 = i0 >> 2;
      uint f = EXPECTED[(i0 & 3) + EXPECTED[(i1 & 63) + EXPECTED[i1 >> 6]]];
      for ( ; f != 0; f >>= 1, ++j)
      {
        if ((f & 1) != 0)
        {
          expected.Add(TOKEN[j]);
        }
      }
    }
    return expected.ToArray();
  }

  private static readonly int[] MAP0 =
  {
    /*   0 */ 71, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 3, 4,
    /*  35 */ 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 18, 18, 18, 18, 18, 18, 18, 18, 20, 21, 22, 23, 24,
    /*  63 */ 25, 6, 26, 27, 28, 29, 30, 31, 32, 32, 33, 32, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 32, 32, 45, 46,
    /*  90 */ 32, 47, 48, 49, 48, 50, 48, 51, 52, 53, 54, 55, 52, 56, 32, 57, 32, 32, 58, 59, 60, 61, 32, 32, 62, 63,
    /* 116 */ 64, 32, 65, 32, 66, 67, 32, 48, 68, 48, 48, 48
  };

  private static readonly int[] MAP1 =
  {
    /*   0 */ 108, 124, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 156, 181, 181, 181, 181,
    /*  21 */ 181, 214, 215, 213, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214,
    /*  42 */ 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214,
    /*  63 */ 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214,
    /*  84 */ 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214, 214,
    /* 105 */ 214, 214, 214, 247, 261, 277, 293, 309, 325, 341, 357, 394, 394, 394, 386, 442, 434, 442, 434, 442, 442,
    /* 126 */ 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 411, 411, 411, 411, 411, 411, 411,
    /* 147 */ 427, 442, 442, 442, 442, 442, 442, 442, 442, 370, 394, 394, 395, 393, 394, 394, 442, 442, 442, 442, 442,
    /* 168 */ 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 394, 394, 394, 394, 394, 394, 394, 394,
    /* 189 */ 394, 394, 394, 394, 394, 394, 394, 394, 394, 394, 394, 394, 394, 394, 394, 394, 394, 394, 394, 394, 394,
    /* 210 */ 394, 394, 394, 441, 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 442,
    /* 231 */ 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 442, 394, 71, 0, 0, 0, 0, 0, 0, 0,
    /* 255 */ 0, 1, 2, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13,
    /* 289 */ 14, 15, 16, 17, 18, 19, 18, 18, 18, 18, 18, 18, 18, 18, 20, 21, 22, 23, 24, 25, 6, 26, 27, 28, 29, 30, 31,
    /* 316 */ 32, 32, 33, 32, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 32, 32, 45, 46, 32, 47, 48, 49, 48, 50, 48,
    /* 342 */ 51, 52, 53, 54, 55, 52, 56, 32, 57, 32, 32, 58, 59, 60, 61, 32, 32, 62, 63, 64, 32, 65, 32, 66, 67, 32,
    /* 368 */ 48, 68, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 70, 70, 48, 48, 48, 48, 48, 48, 48, 48, 48, 69,
    /* 394 */ 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 48, 69, 69, 69, 69, 69, 69, 69, 69, 69, 69,
    /* 420 */ 69, 69, 69, 69, 69, 69, 69, 70, 70, 70, 70, 70, 70, 70, 70, 70, 70, 70, 70, 70, 70, 48, 70, 70, 70, 70,
    /* 446 */ 70, 70, 70, 70, 70, 70, 70, 70, 70, 70, 70, 70
  };

  private static readonly int[] MAP2 =
  {
    /*  0 */ 57344, 63744, 64976, 65008, 65536, 63743, 64975, 65007, 65533, 1114111, 48, 70, 48, 70, 48
  };

  private static readonly int[] INITIAL =
  {
    /*  0 */ 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
    /* 29 */ 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56,
    /* 56 */ 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72
  };

  private static readonly int[] TRANSITION =
  {
    /*    0 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /*   17 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2304, 2323,
    /*   34 */ 5215, 2322, 2339, 4812, 4178, 2305, 2794, 2305, 2368, 2305, 5429, 2305, 3608, 2305, 2305, 3137, 2305,
    /*   51 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2304, 2323, 5215, 2322,
    /*   68 */ 2392, 4812, 4178, 2305, 2794, 2305, 2368, 2305, 5429, 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305,
    /*   85 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 4042, 4812,
    /*  102 */ 2443, 2305, 2794, 2305, 2305, 2305, 3605, 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305,
    /*  119 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 4560, 2305, 2470, 2979, 4153, 3220, 4178, 2305,
    /*  136 */ 2794, 2305, 2305, 2305, 3605, 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /*  153 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 5447, 5447, 2305, 2488, 4042, 4812, 5384, 2523, 2794, 2305,
    /*  170 */ 2305, 2305, 3605, 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /*  187 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 4042, 4812, 4178, 2305, 2794, 2305, 2305, 2305,
    /*  204 */ 3605, 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /*  221 */ 2305, 2305, 2305, 3353, 3350, 2305, 2305, 3513, 2831, 4178, 2305, 2794, 2305, 2305, 2305, 3605, 2305,
    /*  238 */ 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /*  255 */ 2305, 2305, 2305, 2305, 2305, 4856, 2916, 2305, 2305, 2305, 2305, 2305, 2305, 3605, 2305, 3608, 2305,
    /*  272 */ 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3189,
    /*  289 */ 2305, 2553, 3432, 5052, 3941, 4178, 2305, 2794, 2305, 2305, 2305, 3605, 2305, 3608, 2305, 2305, 3137,
    /*  306 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2557, 2555, 2558,
    /*  323 */ 2574, 3695, 4812, 4178, 2305, 2794, 2305, 2305, 2305, 3605, 2305, 3608, 2305, 2305, 3137, 2305, 2305,
    /*  340 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3789, 2305, 2600, 3891, 2650,
    /*  357 */ 4812, 4178, 2305, 2794, 2305, 2305, 2305, 3605, 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305,
    /*  374 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2635, 2584, 4178,
    /*  391 */ 2305, 2794, 2305, 2305, 2305, 3605, 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305,
    /*  408 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2696, 4812, 4178, 2305, 2794,
    /*  425 */ 2305, 2305, 2305, 3605, 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /*  442 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2736, 2742, 2665, 4812, 4178, 2305, 2794, 2305, 2305,
    /*  459 */ 2305, 3605, 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /*  476 */ 2305, 2305, 2305, 2305, 4809, 4803, 2305, 2305, 4942, 3376, 4178, 2305, 2758, 4015, 2786, 2305, 2407,
    /*  493 */ 2305, 2810, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /*  510 */ 2305, 2305, 4809, 4803, 2305, 2305, 4531, 3376, 4178, 2305, 4624, 4015, 4616, 2305, 3819, 2305, 3608,
    /*  527 */ 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /*  544 */ 2305, 2305, 3307, 3938, 4042, 4812, 2418, 2305, 2794, 2305, 2305, 2305, 3605, 2305, 3608, 2305, 2305,
    /*  561 */ 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 4809, 4803,
    /*  578 */ 2305, 2305, 4942, 3376, 4178, 2828, 4187, 2847, 5176, 3575, 2862, 3863, 3608, 2305, 2305, 3137, 2305,
    /*  595 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2890, 4803, 2305, 2305,
    /*  612 */ 4942, 3376, 4178, 2828, 4187, 2847, 5176, 3575, 2862, 3863, 3608, 2305, 2305, 3137, 2305, 2305, 2305,
    /*  629 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2913, 2427, 5317, 2932, 4942, 4812,
    /*  646 */ 4680, 2965, 3006, 4015, 4616, 2305, 3819, 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305,
    /*  663 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2812, 2305, 2305, 2305, 4042, 4812, 4178, 2305,
    /*  680 */ 3469, 3669, 5194, 2990, 3683, 3402, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /*  697 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3770, 3033, 4812, 2305, 2305, 2305, 2305,
    /*  714 */ 2305, 2305, 3605, 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /*  731 */ 2305, 2305, 2305, 2305, 2305, 5281, 2897, 2305, 2305, 4042, 4812, 4178, 2305, 2794, 2305, 2305, 2305,
    /*  748 */ 3605, 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /*  765 */ 2305, 2305, 2305, 2305, 3110, 3095, 3080, 3124, 3161, 4178, 2305, 2305, 2305, 2305, 2305, 5311, 2305,
    /*  782 */ 3217, 2615, 2305, 3461, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /*  799 */ 2305, 2305, 3236, 3240, 3241, 3257, 4812, 3296, 2305, 2794, 2305, 3323, 2305, 5373, 2305, 3347, 2305,
    /*  816 */ 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369,
    /*  833 */ 2427, 5317, 3392, 4942, 3376, 4680, 3418, 3448, 4015, 4905, 3485, 3501, 3529, 3545, 2305, 3059, 3137,
    /*  850 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369, 2427, 5317,
    /*  867 */ 2932, 4942, 3376, 4680, 3561, 3006, 4015, 4905, 4970, 3501, 3863, 3608, 2305, 2305, 3137, 2305, 2305,
    /*  884 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369, 2427, 5317, 2932, 5474,
    /*  901 */ 3376, 4680, 3591, 3006, 4015, 5109, 4970, 3626, 5078, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305,
    /*  918 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369, 2427, 5317, 2932, 4942, 3654, 2720,
    /*  935 */ 2965, 3711, 4459, 4905, 4970, 3749, 3863, 3786, 2305, 3805, 3137, 2305, 2305, 2305, 2305, 2305, 2305,
    /*  952 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369, 2427, 5317, 3853, 3201, 3376, 4680, 2965, 3879,
    /*  969 */ 4015, 4905, 3907, 3923, 3863, 3957, 3046, 3987, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /*  986 */ 2305, 2305, 2305, 2305, 2305, 2305, 3369, 2427, 5317, 2932, 4942, 3376, 4002, 2965, 3006, 4015, 4905,
    /* 1003 */ 4970, 3501, 2680, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 1020 */ 2305, 2305, 2305, 2305, 3369, 2427, 5317, 2932, 4942, 3376, 4680, 2965, 3006, 4015, 4616, 2305, 3819,
    /* 1037 */ 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 1054 */ 2305, 2305, 3369, 2427, 5317, 2932, 2770, 3376, 4073, 2965, 4031, 4015, 4655, 2619, 3819, 2305, 4058,
    /* 1071 */ 4374, 4398, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 1088 */ 3369, 2427, 5317, 2932, 4942, 3376, 4680, 2965, 3006, 4015, 4616, 2305, 3819, 4869, 3608, 2305, 2305,
    /* 1105 */ 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369, 2427,
    /* 1122 */ 5317, 2932, 4942, 3376, 4680, 2965, 3006, 4086, 4757, 2305, 5041, 2305, 4102, 2305, 2305, 3137, 2305,
    /* 1139 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369, 2427, 5317, 2932,
    /* 1156 */ 4942, 3376, 4126, 2965, 4142, 4015, 4616, 2305, 3819, 5145, 4169, 2305, 2305, 3137, 2305, 2305, 2305,
    /* 1173 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369, 4474, 4915, 2932, 2874, 4203,
    /* 1190 */ 4779, 2965, 4219, 4015, 4843, 2305, 3819, 2305, 3608, 4250, 2454, 3762, 2305, 2305, 2305, 2305, 2305,
    /* 1207 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369, 2427, 5317, 2932, 4942, 3376, 5264, 2965,
    /* 1224 */ 3006, 4015, 5415, 4247, 3819, 2305, 3608, 2472, 2305, 4266, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 1241 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369, 2427, 3837, 4290, 4942, 4306, 4680, 4350, 3006, 4015,
    /* 1258 */ 4366, 2305, 3819, 2305, 3608, 2305, 2503, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 1275 */ 2305, 2305, 2305, 2305, 2305, 3369, 2427, 5317, 2932, 4942, 3376, 4680, 2965, 3006, 4015, 4390, 2305,
    /* 1292 */ 3819, 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 1309 */ 2305, 2305, 2305, 3369, 2427, 5317, 2932, 4942, 3376, 4321, 2965, 4414, 4015, 4616, 2305, 3819, 2305,
    /* 1326 */ 3608, 5120, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 1343 */ 2305, 3369, 2427, 2376, 4490, 4942, 3376, 4680, 4506, 3006, 4015, 4616, 2305, 3819, 2305, 4547, 2305,
    /* 1360 */ 3722, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369,
    /* 1377 */ 2427, 5317, 2932, 4942, 3376, 4680, 2965, 4585, 4601, 4640, 2305, 3971, 4671, 4696, 4569, 2305, 4718,
    /* 1394 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369, 2427, 5317,
    /* 1411 */ 2932, 4942, 4742, 4680, 2965, 3006, 4015, 4616, 2305, 5000, 2305, 3608, 2305, 2305, 3137, 2305, 2305,
    /* 1428 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369, 2427, 5317, 2932, 4942,
    /* 1445 */ 3376, 4680, 2965, 3006, 4015, 4795, 2305, 3819, 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305,
    /* 1462 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369, 2427, 5317, 2932, 4942, 4828, 4680,
    /* 1479 */ 4890, 3006, 4015, 4616, 2305, 4931, 2305, 4958, 5440, 5186, 3137, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 1496 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2942, 2949, 3733, 4812, 4178, 2305, 4274,
    /* 1513 */ 2305, 2305, 2305, 3605, 2305, 3608, 2305, 2306, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 1530 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3733, 4812, 4178, 2305, 2794, 2305, 2305,
    /* 1547 */ 2305, 3605, 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 1564 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3638, 4812, 5255, 2305, 3331, 2305, 2305, 2305, 3605,
    /* 1581 */ 2305, 3608, 2305, 2305, 4770, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 1598 */ 2305, 2305, 2913, 2427, 5317, 2932, 4942, 3376, 4680, 2965, 3006, 4015, 4616, 2305, 3819, 2305, 3608,
    /* 1615 */ 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 1632 */ 3369, 2427, 5317, 2932, 4942, 3376, 4680, 4986, 3006, 4015, 4905, 4970, 3501, 5343, 3608, 2305, 2305,
    /* 1649 */ 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369, 2427,
    /* 1666 */ 5317, 2932, 4942, 3376, 4680, 2965, 3006, 4015, 4905, 4970, 3501, 3863, 3608, 2305, 2305, 3137, 2305,
    /* 1683 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369, 2427, 5317, 2932,
    /* 1700 */ 4942, 3376, 4680, 5027, 3006, 4015, 4905, 4970, 3501, 3863, 3608, 2305, 2305, 3137, 2305, 2305, 2305,
    /* 1717 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369, 2427, 5317, 2932, 4942, 3376,
    /* 1734 */ 4680, 2965, 3006, 4015, 4905, 2352, 3501, 3863, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305,
    /* 1751 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369, 2537, 5317, 5068, 4942, 5094, 4680, 2965,
    /* 1768 */ 3006, 4015, 4905, 4970, 3501, 3863, 3608, 2305, 2305, 2711, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 1785 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369, 2427, 5317, 2932, 4942, 3376, 4680, 2965, 3006, 4015,
    /* 1802 */ 4616, 2305, 3819, 2305, 3608, 3610, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 1819 */ 2305, 2305, 2305, 2305, 2305, 3369, 2427, 5317, 2932, 4942, 3376, 4680, 2965, 3006, 4015, 4616, 3064,
    /* 1836 */ 3819, 3145, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 1853 */ 2305, 2305, 2305, 3369, 2427, 5317, 2932, 4942, 3376, 4680, 2965, 3006, 4015, 4616, 2305, 4520, 2305,
    /* 1870 */ 3608, 5142, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 1887 */ 2305, 3369, 2427, 5317, 2932, 4942, 3376, 4680, 2965, 3006, 4015, 5246, 2305, 3819, 2305, 3608, 2305,
    /* 1904 */ 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369,
    /* 1921 */ 2427, 5126, 2932, 4942, 5161, 4680, 2965, 3006, 4429, 4616, 2305, 3819, 2305, 3608, 5210, 2507, 3137,
    /* 1938 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369, 2427, 5317,
    /* 1955 */ 2932, 4942, 5231, 4680, 2965, 3006, 4444, 4616, 2305, 3819, 4874, 3608, 2305, 5280, 3137, 2305, 2305,
    /* 1972 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369, 2427, 5317, 2932, 4942,
    /* 1989 */ 3376, 4680, 5297, 3006, 4015, 4616, 2305, 3819, 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305,
    /* 2006 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369, 4231, 4110, 5333, 4942, 3376, 4680,
    /* 2023 */ 5359, 3006, 4334, 4616, 2305, 3819, 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 2040 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 3369, 2427, 5317, 2932, 4942, 5400, 4680, 2965, 3006,
    /* 2057 */ 4015, 4616, 2305, 3819, 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 2074 */ 2305, 2305, 2305, 2305, 2305, 2305, 3369, 3017, 5317, 2932, 4942, 3376, 4680, 2965, 3006, 4015, 4616,
    /* 2091 */ 2305, 3819, 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 2108 */ 2305, 2305, 2305, 2305, 3369, 2427, 5317, 2932, 4942, 3376, 4680, 2965, 5463, 5490, 4616, 2305, 3819,
    /* 2125 */ 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 2142 */ 2305, 2305, 3369, 2427, 4702, 2932, 4942, 3376, 4680, 2965, 3006, 4015, 4616, 2305, 3819, 2305, 3608,
    /* 2159 */ 2305, 2305, 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 2176 */ 2305, 3830, 3272, 3280, 3176, 4812, 4178, 2305, 2794, 2305, 2305, 2305, 3605, 2305, 3608, 2305, 2305,
    /* 2193 */ 3137, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 4809, 4803,
    /* 2210 */ 2305, 2305, 5011, 4812, 4178, 2305, 4624, 4015, 4616, 2305, 3819, 2305, 3608, 2305, 2305, 3137, 2305,
    /* 2227 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2913, 2427, 5317, 2932,
    /* 2244 */ 5011, 4812, 4680, 2965, 3006, 4015, 4616, 2305, 3819, 2305, 3608, 2305, 2305, 3137, 2305, 2305, 2305,
    /* 2261 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 4726, 2305, 2305,
    /* 2278 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305,
    /* 2295 */ 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 2305, 1537, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    /* 2320 */ 0, 277, 1537, 0, 0, 0, 0, 0, 0, 0, 1537, 1537, 1537, 1537, 1537, 1537, 0, 0, 0, 1537, 1537, 1537, 1537,
    /* 2343 */ 0, 5224, 1537, 0, 0, 74, 75, 76, 77, 0, 0, 0, 0, 215, 0, 216, 0, 0, 218, 219, 0, 0, 0, 0, 223, 0, 0, 197,
    /* 2371 */ 0, 0, 0, 0, 197, 0, 0, 0, 0, 0, 0, 0, 0, 0, 91, 0, 2121, 0, 0, 0, 0, 1537, 1537, 1537, 1537, 0, 5224,
    /* 2398 */ 1537, 0, 0, 74, 75, 76, 77, 78, 79, 0, 0, 0, 225, 197, 0, 0, 0, 0, 0, 163, 0, 0, 0, 0, 0, 0, 20480, 5224,
    /* 2426 */ 5224, 0, 0, 0, 0, 0, 0, 0, 2121, 2563, 0, 0, 0, 0, 2121, 2121, 2121, 0, 129, 0, 134, 134, 0, 137, 5224,
    /* 2451 */ 5224, 0, 140, 0, 0, 0, 0, 0, 0, 274, 0, 0, 0, 0, 29184, 0, 0, 0, 0, 76, 9216, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    /* 2481 */ 0, 0, 0, 0, 0, 268, 0, 0, 82, 0, 0, 0, 0, 0, 0, 0, 82, 0, 0, 0, 0, 99, 0, 0, 0, 271, 0, 0, 0, 0, 0, 0, 0,
    /* 2514 */ 0, 0, 0, 0, 0, 276, 0, 0, 0, 148, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 158, 0, 160, 0, 0, 0, 85, 0, 0, 0, 2121,
    /* 2545 */ 2563, 0, 0, 0, 0, 2121, 2121, 2121, 77, 12800, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 13312, 0, 0, 0,
    /* 2573 */ 0, 0, 13312, 0, 0, 13312, 0, 0, 0, 0, 13312, 0, 0, 0, 0, 0, 0, 0, 14336, 0, 0, 0, 0, 0, 94, 95, 0, 0, 0,
    /* 2602 */ 13824, 13824, 88, 0, 0, 0, 0, 0, 0, 0, 13824, 13824, 88, 0, 0, 0, 5632, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    /* 2630 */ 0, 221, 0, 0, 0, 0, 14848, 0, 14848, 0, 5224, 0, 0, 0, 74, 75, 76, 77, 78, 79, 0, 0, 0, 13824, 0, 5224,
    /* 2656 */ 0, 0, 0, 74, 75, 76, 77, 78, 79, 0, 0, 0, 15872, 0, 5224, 0, 0, 0, 74, 75, 76, 77, 78, 79, 0, 0, 0,
    /* 2683 */ 26861, 0, 0, 215, 216, 0, 0, 0, 0, 0, 0, 223, 224, 0, 15360, 0, 15360, 0, 5224, 0, 0, 0, 74, 75, 76, 77,
    /* 2709 */ 78, 79, 0, 0, 0, 32768, 277, 0, 277, 277, 0, 0, 0, 0, 0, 0, 0, 0, 5224, 5224, 139, 0, 141, 0, 26255, 0,
    /* 2735 */ 147, 0, 0, 15872, 0, 0, 0, 0, 0, 0, 0, 0, 0, 15872, 0, 0, 0, 0, 0, 0, 15872, 0, 0, 162, 0, 0, 0, 0, 162,
    /* 2764 */ 0, 0, 162, 5224, 139, 162, 0, 0, 0, 0, 0, 5224, 0, 110, 2121, 74, 75, 76, 77, 78, 79, 0, 0, 196, 163, 0,
    /* 2790 */ 0, 0, 0, 163, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5224, 0, 0, 0, 0, 0, 0, 244, 197, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    /* 2822 */ 0, 0, 0, 0, 16896, 0, 0, 7282, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 123, 125, 0, 0, 180, 147, 182,
    /* 2851 */ 149, 0, 0, 0, 0, 0, 0, 0, 157, 191, 159, 193, 224, 0, 196, 197, 0, 0, 0, 0, 0, 163, 204, 0, 0, 0, 0, 0,
    /* 2879 */ 5224, 0, 111, 2121, 74, 75, 76, 77, 78, 79, 0, 0, 0, 2563, 0, 0, 0, 80, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    /* 2907 */ 21504, 0, 0, 0, 0, 0, 0, 2121, 2563, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 124, 126, 0, 0, 2121, 2121,
    /* 2935 */ 0, 0, 0, 0, 0, 0, 2121, 0, 0, 0, 0, 0, 0, 0, 30720, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 30720, 149,
    /* 2966 */ 0, 0, 0, 0, 0, 0, 0, 0, 0, 157, 157, 159, 159, 0, 0, 0, 94, 0, 0, 0, 0, 0, 0, 9290, 0, 0, 0, 74, 0, 75,
    /* 2996 */ 0, 0, 0, 0, 0, 0, 0, 0, 94, 0, 0, 163, 0, 0, 0, 0, 163, 0, 0, 5224, 139, 0, 0, 0, 0, 0, 87, 0, 2121,
    /* 3025 */ 2563, 0, 0, 0, 0, 2121, 2121, 2121, 17508, 0, 17509, 0, 17511, 17511, 107, 0, 0, 0, 0, 76, 77, 0, 0, 0,
    /* 3049 */ 0, 258, 0, 0, 0, 0, 263, 0, 0, 266, 0, 0, 0, 0, 272, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 220, 0, 0, 0, 0,
    /* 3080 */ 22016, 0, 22016, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 22016, 0, 0, 0, 0, 22016, 22016, 22016, 0, 0, 0, 0,
    /* 3107 */ 0, 0, 0, 22016, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 22016, 0, 0, 0, 22016, 0, 0, 0, 5224, 0, 0, 0, 74, 75, 76,
    /* 3136 */ 77, 0, 0, 0, 0, 277, 0, 277, 277, 0, 0, 0, 0, 0, 0, 0, 0, 0, 241, 0, 0, 0, 0, 0, 0, 0, 0, 23040, 0, 0, 0,
    /* 3167 */ 0, 0, 16384, 0, 0, 0, 0, 94, 95, 0, 0, 0, 34304, 0, 5224, 0, 0, 0, 74, 75, 76, 77, 0, 0, 0, 75, 77, 79,
    /* 3195 */ 0, 0, 0, 0, 0, 12800, 0, 0, 0, 0, 0, 5224, 0, 109, 2121, 74, 75, 76, 77, 78, 79, 0, 196, 6144, 0, 0, 0,
    /* 3222 */ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3072, 95, 0, 0, 83, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 83, 0, 0, 0, 0, 0, 0,
    /* 3255 */ 0, 0, 0, 22528, 0, 22528, 0, 5224, 0, 0, 0, 74, 75, 76, 77, 78, 79, 0, 0, 0, 34304, 34304, 0, 0, 0, 0, 0,
    /* 3282 */ 0, 0, 0, 34304, 34304, 0, 0, 0, 0, 0, 34304, 34304, 0, 0, 0, 130, 0, 130, 135, 0, 130, 5224, 5224, 0,
    /* 3306 */ 130, 0, 0, 0, 0, 0, 89, 0, 0, 0, 0, 0, 0, 0, 0, 0, 89, 0, 0, 198, 0, 0, 0, 0, 198, 0, 0, 0, 0, 0, 0, 0,
    /* 3338 */ 0, 0, 5258, 0, 0, 0, 0, 0, 0, 196, 226, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11776, 0, 0, 0, 0, 0,
    /* 3369 */ 0, 2121, 2563, 0, 0, 0, 0, 8785, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 94, 95, 0, 0, 2121, 2121, 0, 96, 0,
    /* 3398 */ 0, 0, 0, 2121, 0, 0, 0, 0, 0, 0, 74, 75, 0, 0, 0, 0, 0, 0, 94, 95, 149, 0, 0, 151, 0, 0, 0, 0, 0, 0, 157,
    /* 3429 */ 157, 159, 159, 0, 0, 0, 95, 0, 0, 0, 0, 0, 0, 12875, 0, 0, 0, 75, 0, 0, 163, 0, 0, 0, 0, 163, 0, 0, 5224,
    /* 3458 */ 139, 172, 175, 0, 0, 0, 0, 277, 0, 277, 6656, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5224, 8192, 0, 0, 0, 0, 0, 0, 0,
    /* 3487 */ 214, 0, 215, 0, 216, 217, 27648, 0, 0, 0, 0, 0, 0, 223, 0, 224, 0, 196, 197, 0, 0, 0, 0, 0, 163, 204, 0,
    /* 3514 */ 0, 0, 0, 0, 5224, 106, 0, 0, 74, 75, 76, 77, 78, 79, 0, 0, 24064, 0, 0, 0, 0, 215, 216, 0, 0, 0, 0, 0, 0,
    /* 3543 */ 223, 224, 196, 197, 0, 0, 0, 0, 248, 0, 0, 0, 252, 0, 0, 0, 0, 10752, 149, 0, 0, 0, 0, 0, 0, 155, 0, 0,
    /* 3571 */ 157, 157, 159, 159, 0, 0, 0, 180, 215, 182, 216, 0, 0, 0, 0, 0, 0, 0, 191, 223, 149, 0, 150, 0, 0, 0, 0,
    /* 3598 */ 0, 0, 0, 157, 157, 159, 159, 0, 0, 0, 196, 197, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 31744, 0,
    /* 3627 */ 224, 0, 196, 197, 0, 0, 0, 0, 230, 163, 204, 0, 0, 0, 0, 0, 5225, 31232, 0, 0, 74, 75, 76, 77, 0, 0, 0,
    /* 3654 */ 8785, 0, 0, 116, 0, 0, 0, 0, 0, 0, 0, 0, 0, 94, 95, 0, 0, 74, 0, 75, 0, 0, 0, 0, 0, 0, 0, 94, 0, 95, 0,
    /* 3685 */ 0, 196, 197, 0, 0, 0, 0, 0, 0, 7680, 0, 0, 0, 0, 0, 5224, 0, 13312, 0, 74, 75, 76, 77, 78, 79, 0, 0, 163,
    /* 3713 */ 0, 0, 0, 167, 163, 0, 0, 5224, 139, 0, 0, 0, 0, 0, 273, 0, 0, 0, 25088, 28672, 0, 0, 0, 0, 0, 5224, 0, 0,
    /* 3741 */ 0, 74, 75, 76, 77, 0, 0, 0, 0, 224, 0, 196, 197, 0, 0, 0, 0, 0, 163, 204, 231, 0, 0, 0, 0, 277, 19968,
    /* 3768 */ 277, 277, 0, 0, 0, 0, 0, 0, 0, 0, 98, 0, 0, 17506, 0, 0, 0, 0, 196, 197, 9728, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    /* 3798 */ 0, 0, 0, 0, 13824, 0, 0, 0, 10240, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 11264, 0, 0, 0, 196, 197, 0, 0, 0, 0,
    /* 3828 */ 0, 163, 0, 0, 0, 0, 0, 0, 34304, 0, 0, 0, 0, 0, 0, 0, 0, 0, 90, 0, 2121, 0, 0, 0, 0, 0, 2121, 2121, 0,
    /* 3857 */ 97, 0, 0, 0, 0, 2121, 0, 0, 0, 0, 0, 0, 215, 216, 0, 0, 0, 0, 0, 0, 223, 224, 0, 163, 0, 0, 166, 0, 163,
    /* 3886 */ 0, 0, 5224, 139, 173, 0, 0, 0, 0, 0, 13912, 13824, 0, 0, 0, 0, 0, 13912, 13824, 0, 0, 212, 0, 0, 0, 215,
    /* 3912 */ 0, 216, 0, 0, 0, 0, 0, 0, 222, 0, 223, 0, 224, 0, 196, 197, 0, 227, 0, 0, 0, 163, 204, 0, 0, 233, 0, 0,
    /* 3940 */ 89, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 94, 3072, 0, 196, 197, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 254,
    /* 3971 */ 0, 0, 0, 196, 197, 0, 0, 0, 0, 0, 163, 0, 0, 232, 0, 234, 0, 0, 270, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    /* 4001 */ 18432, 0, 0, 131, 0, 0, 0, 0, 5224, 5224, 139, 0, 0, 0, 0, 0, 147, 0, 149, 0, 0, 0, 0, 0, 0, 0, 157, 0,
    /* 4029 */ 159, 0, 0, 163, 164, 0, 0, 0, 163, 0, 0, 5224, 139, 0, 0, 0, 0, 0, 5224, 0, 0, 0, 74, 75, 76, 77, 78, 79,
    /* 4057 */ 0, 196, 197, 0, 245, 246, 0, 0, 0, 0, 251, 0, 253, 0, 0, 255, 0, 0, 132, 0, 0, 0, 0, 5224, 5224, 139, 0,
    /* 4084 */ 0, 0, 0, 0, 147, 0, 149, 0, 0, 0, 0, 0, 189, 0, 157, 0, 159, 0, 196, 197, 0, 0, 0, 0, 0, 249, 0, 0, 0, 0,
    /* 4114 */ 0, 0, 0, 0, 86, 0, 0, 2121, 0, 0, 0, 0, 128, 0, 0, 0, 0, 0, 0, 5224, 5224, 139, 0, 0, 0, 0, 144, 147, 0,
    /* 4143 */ 163, 0, 165, 0, 0, 163, 0, 0, 5224, 139, 0, 0, 0, 0, 0, 5224, 0, 0, 0, 3584, 75, 4096, 77, 4608, 0, 0,
    /* 4169 */ 196, 197, 0, 0, 0, 0, 0, 0, 250, 0, 0, 0, 0, 0, 0, 0, 5224, 5224, 0, 0, 0, 0, 0, 0, 0, 169, 0, 5224, 139,
    /* 4198 */ 0, 0, 0, 0, 0, 8785, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 94, 95, 127, 0, 163, 0, 0, 0, 0, 163, 0, 0,
    /* 4228 */ 5224, 139, 174, 0, 0, 0, 0, 86, 0, 0, 2121, 2563, 0, 0, 0, 0, 2121, 2121, 2121, 0, 213, 0, 0, 0, 0, 0, 0,
    /* 4255 */ 0, 0, 0, 0, 0, 0, 0, 0, 28427, 0, 0, 0, 0, 278, 0, 277, 0, 277, 277, 0, 0, 0, 0, 0, 0, 0, 0, 171, 5224,
    /* 4284 */ 0, 0, 0, 0, 0, 0, 0, 2121, 2121, 90, 0, 0, 0, 0, 0, 2121, 0, 0, 0, 0, 0, 90, 8785, 115, 0, 0, 0, 0, 0, 0,
    /* 4314 */ 0, 0, 0, 0, 0, 94, 95, 0, 0, 133, 0, 0, 0, 0, 5224, 5224, 139, 0, 0, 0, 0, 0, 147, 0, 149, 0, 0, 0, 0,
    /* 4343 */ 188, 0, 0, 157, 0, 159, 0, 149, 0, 0, 0, 0, 0, 0, 0, 0, 0, 157, 157, 159, 159, 0, 161, 0, 0, 163, 0, 200,
    /* 4371 */ 0, 0, 163, 0, 0, 0, 0, 0, 0, 0, 0, 262, 0, 0, 0, 0, 0, 0, 0, 0, 0, 163, 0, 0, 201, 0, 163, 0, 0, 0, 0, 0,
    /* 4403 */ 0, 0, 0, 275, 0, 0, 0, 0, 0, 0, 0, 0, 163, 0, 0, 0, 0, 163, 0, 0, 5224, 139, 0, 0, 0, 177, 0, 0, 147, 0,
    /* 4433 */ 149, 0, 0, 0, 187, 0, 0, 0, 157, 0, 159, 0, 0, 147, 0, 149, 0, 0, 186, 0, 0, 0, 0, 157, 0, 159, 0, 0,
    /* 4461 */ 147, 0, 149, 184, 0, 0, 0, 0, 0, 0, 157, 0, 159, 0, 0, 84, 0, 0, 0, 0, 2121, 2563, 0, 0, 0, 0, 2121,
    /* 4488 */ 2121, 2121, 0, 2121, 2121, 91, 0, 0, 0, 0, 0, 2121, 0, 0, 0, 0, 0, 91, 149, 0, 0, 0, 0, 0, 0, 0, 156, 0,
    /* 4516 */ 157, 157, 159, 159, 0, 0, 0, 196, 197, 0, 0, 0, 0, 0, 21155, 0, 0, 0, 0, 0, 5224, 0, 0, 2121, 74, 75, 76,
    /* 4543 */ 77, 78, 79, 114, 196, 197, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 27136, 0, 0, 0, 74, 76, 78, 0, 0, 9216, 0, 0, 0,
    /* 4572 */ 0, 0, 0, 0, 261, 0, 0, 264, 265, 0, 0, 0, 0, 0, 163, 0, 0, 0, 0, 163, 0, 0, 5224, 139, 0, 0, 176, 0, 178,
    /* 4601 */ 179, 0, 147, 0, 149, 0, 185, 0, 0, 0, 0, 190, 157, 0, 159, 0, 0, 163, 0, 0, 0, 0, 163, 0, 0, 0, 0, 0, 0,
    /* 4630 */ 0, 0, 0, 5224, 139, 0, 0, 0, 0, 0, 195, 0, 163, 0, 0, 0, 0, 163, 0, 0, 0, 206, 0, 0, 210, 0, 0, 163, 0,
    /* 4659 */ 0, 0, 0, 163, 0, 0, 0, 0, 0, 0, 0, 211, 235, 0, 236, 0, 0, 239, 0, 0, 240, 0, 0, 0, 0, 0, 0, 0, 5224,
    /* 4688 */ 5224, 139, 0, 0, 0, 0, 0, 147, 196, 197, 0, 0, 0, 247, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 93, 2121, 0, 0, 0,
    /* 4717 */ 0, 17920, 18944, 0, 0, 277, 0, 277, 277, 0, 0, 0, 0, 0, 0, 0, 0, 1024, 0, 0, 0, 0, 0, 0, 0, 8785, 0, 0,
    /* 4745 */ 0, 0, 0, 0, 0, 0, 120, 0, 0, 0, 94, 95, 0, 0, 163, 0, 0, 0, 0, 163, 0, 0, 0, 0, 207, 0, 0, 0, 0, 279, 0,
    /* 4776 */ 280, 280, 0, 0, 0, 0, 0, 0, 0, 0, 5224, 5224, 139, 0, 0, 142, 0, 0, 147, 0, 0, 163, 199, 0, 0, 0, 163, 0,
    /* 4804 */ 0, 0, 0, 0, 0, 0, 0, 2563, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 94, 95, 0, 8785, 0, 0, 0, 0, 0, 0, 0,
    /* 4836 */ 0, 0, 121, 0, 0, 94, 95, 0, 0, 163, 0, 0, 0, 0, 163, 0, 0, 0, 0, 208, 0, 0, 0, 0, 12390, 12390, 0, 0, 0,
    /* 4865 */ 112, 113, 76, 77, 0, 0, 0, 0, 238, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 243, 0, 0, 0, 0, 149, 0, 0, 0, 0, 0,
    /* 4896 */ 0, 0, 0, 0, 157, 157, 159, 159, 23552, 0, 0, 163, 0, 0, 0, 0, 163, 0, 204, 0, 0, 0, 0, 0, 0, 84, 0, 0, 0,
    /* 4925 */ 0, 2121, 0, 0, 0, 0, 0, 0, 24576, 196, 197, 0, 0, 0, 0, 0, 163, 0, 0, 0, 0, 0, 5224, 0, 0, 2121, 74, 75,
    /* 4953 */ 76, 77, 78, 79, 0, 196, 197, 0, 0, 0, 0, 0, 0, 0, 0, 0, 25600, 0, 0, 0, 0, 215, 0, 216, 0, 0, 0, 0, 0, 0,
    /* 4983 */ 0, 0, 223, 149, 0, 0, 0, 0, 153, 0, 0, 0, 0, 157, 157, 159, 159, 0, 0, 0, 196, 197, 0, 0, 0, 229, 0, 163,
    /* 5011 */ 0, 0, 0, 0, 0, 5224, 0, 0, 2121, 74, 75, 76, 77, 0, 0, 0, 149, 0, 0, 0, 152, 0, 0, 0, 0, 0, 157, 157,
    /* 5039 */ 159, 159, 0, 0, 0, 196, 197, 0, 0, 228, 0, 0, 163, 0, 0, 0, 0, 0, 5224, 0, 0, 0, 74, 3584, 76, 4096, 78,
    /* 5066 */ 4608, 0, 0, 2121, 2121, 0, 0, 0, 0, 85, 0, 2121, 0, 0, 0, 0, 0, 0, 215, 216, 0, 0, 0, 0, 29696, 0, 223,
    /* 5093 */ 224, 8785, 0, 0, 0, 0, 0, 119, 0, 0, 0, 0, 0, 122, 94, 95, 0, 0, 163, 0, 0, 0, 0, 163, 0, 204, 205, 0, 0,
    /* 5122 */ 0, 0, 0, 259, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 92, 2121, 0, 0, 0, 0, 0, 257, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    /* 5154 */ 0, 0, 0, 0, 30208, 0, 0, 8785, 0, 0, 0, 117, 0, 0, 0, 0, 0, 0, 0, 0, 94, 95, 0, 0, 163, 0, 0, 0, 0, 163,
    /* 5184 */ 169, 204, 0, 0, 0, 0, 0, 0, 0, 19456, 0, 0, 0, 0, 0, 0, 0, 0, 7680, 0, 0, 0, 0, 0, 0, 0, 256, 0, 33280,
    /* 5213 */ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1537, 1537, 1537, 1537, 1537, 8785, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    /* 5242 */ 32256, 0, 94, 95, 0, 0, 163, 0, 0, 0, 0, 203, 0, 0, 0, 0, 0, 0, 0, 0, 5225, 5258, 0, 0, 0, 0, 0, 0, 0,
    /* 5271 */ 5224, 5224, 139, 0, 0, 0, 0, 145, 147, 269, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 21504, 149, 0,
    /* 5299 */ 0, 0, 0, 0, 154, 0, 0, 0, 157, 157, 159, 159, 0, 0, 0, 196, 197, 6144, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
    /* 5328 */ 2121, 0, 0, 0, 0, 0, 2121, 2121, 0, 0, 0, 0, 86, 0, 2121, 0, 0, 0, 0, 0, 0, 215, 216, 0, 0, 242, 0, 0, 0,
    /* 5357 */ 223, 224, 149, 0, 0, 0, 0, 0, 0, 0, 0, 33792, 157, 157, 159, 159, 0, 0, 0, 196, 226, 0, 0, 0, 0, 0, 198,
    /* 5384 */ 0, 0, 0, 0, 0, 136, 0, 5224, 5224, 0, 0, 0, 0, 0, 0, 146, 8785, 0, 0, 0, 0, 118, 0, 0, 0, 0, 0, 0, 0, 94,
    /* 5414 */ 95, 0, 0, 163, 0, 0, 0, 202, 163, 0, 0, 0, 0, 0, 209, 0, 0, 0, 196, 197, 0, 0, 0, 0, 0, 197, 0, 0, 0, 0,
    /* 5444 */ 0, 0, 260, 0, 0, 0, 0, 0, 0, 0, 0, 0, 82, 0, 0, 0, 0, 0, 0, 0, 163, 0, 0, 0, 0, 168, 170, 0, 5224, 139,
    /* 5474 */ 0, 0, 0, 0, 0, 5224, 0, 108, 2121, 74, 75, 76, 77, 78, 79, 0, 0, 181, 147, 183, 149, 0, 0, 0, 0, 0, 0, 0,
    /* 5502 */ 157, 192, 159, 194
  };

  private static readonly uint[] EXPECTED =
  {
    /*   0 */ 4, 68, 132, 188, 222, 206, 215, 303, 305, 211, 219, 241, 251, 231, 484, 255, 259, 271, 278, 285, 281, 300,
    /*  22 */ 244, 247, 209, 228, 232, 295, 312, 315, 291, 411, 225, 305, 294, 296, 267, 405, 319, 305, 413, 328, 305,
    /*  43 */ 262, 266, 336, 408, 305, 411, 414, 305, 294, 264, 381, 337, 410, 305, 412, 210, 294, 267, 385, 305, 458,
    /*  64 */ 438, 384, 305, 305, 335, 410, 305, 341, 305, 350, 305, 305, 305, 288, 360, 232, 357, 344, 322, 346, 354,
    /*  85 */ 438, 370, 374, 233, 274, 378, 389, 305, 305, 402, 363, 445, 424, 393, 399, 324, 418, 330, 443, 304, 425,
    /* 106 */ 305, 395, 323, 431, 437, 331, 444, 305, 423, 394, 237, 429, 435, 305, 362, 303, 421, 305, 395, 476, 430,
    /* 127 */ 436, 442, 303, 305, 366, 478, 449, 363, 365, 477, 462, 364, 476, 466, 473, 470, 482, 305, 305, 305, 305,
    /* 148 */ 305, 452, 305, 305, 237, 457, 234, 455, 305, 455, 305, 456, 237, 305, 305, 305, 305, 307, 305, 236, 305,
    /* 169 */ 305, 305, 305, 305, 307, 235, 305, 305, 305, 305, 305, 305, 305, 308, 305, 305, 305, 305, 305, 305, 305,
    /* 190 */ 306, 305, 305, 305, 305, 305, 307, 305, 305, 305, 305, 305, 305, 305, 305, 305, 128, 256, 8192, 65536,
    /* 210 */ 1048576, 0, 0, 0, 12, 131072, 1048576, 4194304, 16777216, 20, 1048580, 4, 4, 8, 16, 64, 8192, 1048576, 0,
    /* 229 */ 0, 0, 134217728, 2147483648, 0, 0, 0, 2, 0, 0, 0, 4, 4, 4194312, 33554440, 8, 64, 64, 128, 256, 256, 8192,
    /* 251 */ 128, 16908288, 1140850688, 67108864, 1140850692, 67108868, 134217732, 2147483652, 4, 34603016,
    /* 261 */ 2147483656, 32, 32, 32, 32, 0, 1024, 2048, 262144, 524288, 33554432, 201326592, 1140850688, 0, 0, 0,
    /* 277 */ 234882048, 3078, 34603020, 16908356, 3076, 805306372, 3076, 1946157060, 201326596, 1140850692, 2883648, 0,
    /* 289 */ 0, 1, 512, 32768, 3072, 0, 0, 32, 32, 0, 0, 8412160, 8412672, 35844, 33554432, 67108864, 0, 0, 0, 0, 1, 0,
    /* 311 */ 0, 0, 3072, 2883584, 3072, 16384, 7168, 512, 5120, 512, 32768, 1024, 0, 0, 0, 116, 16384, 64, 1048576, 0,
    /* 331 */ 0, 0, 2097152, 536870912, 0, 524288, 2097152, 0, 2048, 16384, 0, 2097152, 0, 4096, 0, 0, 1024, 2098176,
    /* 349 */ 134218752, 4096, 0, 4096, 4096, 2147487744, 100663296, 1073741824, 0, 0, 512, 1024, 4096, 2097152,
    /* 363 */ 536870912, 2147483648, 0, 0, 8, 0, 4, 134218752, 0, 1024, 100663296, 40960, 0, 0, 2684358656, 10, 3072,
    /* 380 */ 266, 2048, 2048, 262144, 524288, 2097152, 0, 4096, 0, 130, 130, 268435572, 31408128, 32768, 0, 0, 8, 256,
    /* 398 */ 0, 264, 0, 128, 0, 0, 4096, 2097152, 1024, 2048, 16384, 16384, 4096, 0, 0, 0, 64, 64, 64, 1048576, 196608,
    /* 419 */ 1835008, 29360128, 0, 0, 32768, 0, 0, 0, 8192, 32768, 48, 64, 16384, 196608, 1572864, 12582912, 1572864,
    /* 436 */ 12582912, 16777216, 0, 0, 0, 1024, 0, 536870912, 2147483648, 0, 33554432, 67108864, 1073741824, 1048576,
    /* 450 */ 12582912, 16777216, 0, 1, 4, 0, 4, 4, 0, 0, 0, 1048576, 65536, 12582912, 16777216, 536870912, 64, 65536,
    /* 468 */ 8388608, 16777216, 4, 16, 64, 2147483648, 0, 8, 0, 4, 16, 32, 64, 196608, 0, 64, 0, 0, 0, 33554444
  };

  private static readonly String[] TOKEN =
  {
    "(0)",
    "EOF",
    "S",
    "Name",
    "Nmtoken",
    "EntityValue",
    "AttValue",
    "SystemLiteral",
    "PubidLiteral",
    "CharData",
    "Comment",
    "PI",
    "CDSect",
    "VersionNum",
    "CharRef",
    "PEReference",
    "EncName",
    "'\"'",
    "'#FIXED'",
    "'#IMPLIED'",
    "'#PCDATA'",
    "'#REQUIRED'",
    "'%'",
    "'&'",
    "''''",
    "'('",
    "')'",
    "')*'",
    "'*'",
    "'+'",
    "','",
    "'/>'",
    "';'",
    "'<'",
    "'<!ATTLIST'",
    "'<!DOCTYPE'",
    "'<!ELEMENT'",
    "'<!ENTITY'",
    "'<!NOTATION'",
    "'</'",
    "'<?xml'",
    "'='",
    "'>'",
    "'?'",
    "'?>'",
    "'ANY'",
    "'CDATA'",
    "'EMPTY'",
    "'ENTITIES'",
    "'ENTITY'",
    "'ID'",
    "'IDREF'",
    "'IDREFS'",
    "'NDATA'",
    "'NMTOKEN'",
    "'NMTOKENS'",
    "'NOTATION'",
    "'PUBLIC'",
    "'SYSTEM'",
    "'['",
    "']'",
    "'encoding'",
    "'no'",
    "'standalone'",
    "'version'",
    "'yes'",
    "'|'"
  };
}

// End
