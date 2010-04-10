
using System;

namespace bibliographer
{

	public static class StringOps
	{
		public static string TeXToUnicode(string str)
		{

			if (str != null)
			{
				// Get rid of braces

				str = str.Replace("{","");
				str = str.Replace("}","");

				if (str.Contains("\\"))
				{
					str = str.Replace("\\`A","À");
					str = str.Replace("\\'A","Á");
					str = str.Replace("\\^A","Â");
					str = str.Replace("\\~A","Ã");
					str = str.Replace("\\\"A","Ä");
					str = str.Replace("\\AA","Å");
					str = str.Replace("\\rA","Å");
					str = str.Replace("\\AE","Æ");
					str = str.Replace("\\cC","Ç");

					str = str.Replace("\\`E","È");
					str = str.Replace("\\'E","É");
					str = str.Replace("\\^E","Ê");
					str = str.Replace("\\\"E","Ë");

					str = str.Replace("\\`I","Ì");
					str = str.Replace("\\'I","Í");
					str = str.Replace("\\^I","Î");
					str = str.Replace("\\\"I","Ï");

					str = str.Replace("\\~N","Ñ");

					str = str.Replace("\\`O","Ò");
					str = str.Replace("\\'O","Ó");
					str = str.Replace("\\^O","Ô");
					str = str.Replace("\\~O","Õ");
					str = str.Replace("\\\"O","Ö");
					str = str.Replace("\\O","Ø");

					str = str.Replace("\\`U","Ù");
					str = str.Replace("\\'U","Ú");
					str = str.Replace("\\^U","Û");
					str = str.Replace("\\\"U","Ü");

					str = str.Replace("\\'Y","Ý");

					str = str.Replace("\\ss","ß");

					str = str.Replace("\\`a","à");
					str = str.Replace("\\'a","á");
					str = str.Replace("\\^a","â");
					str = str.Replace("\\~a","ã");
					str = str.Replace("\\\"a","ä");
					str = str.Replace("\\aa","å");
					str = str.Replace("\\ra","å");

					str = str.Replace("\\ae","æ");

					str = str.Replace("\\cc","ç");

					str = str.Replace("\\`e","è");
					str = str.Replace("\\'e","é");
					str = str.Replace("\\^e","ê");
					str = str.Replace("\\\"e","ë");

					str = str.Replace("\\`\\i","ì");
					str = str.Replace("\\'\\i","í");
					str = str.Replace("\\^\\i","î");
					str = str.Replace("\\\"\\i","ï");

					str = str.Replace("\\`i","ì");
					str = str.Replace("\\'i","í");
					str = str.Replace("\\^i","î");
					str = str.Replace("\\\"i","ï");

					//str = str.Replace("","ð");

					str = str.Replace("\\~n","ñ");

					str = str.Replace("\\`o","ò");
					str = str.Replace("\\'o","ó");
					str = str.Replace("\\^o","ô");
					str = str.Replace("\\~o","õ");
					str = str.Replace("\\\"o","ö");
					str = str.Replace("\\o","ø");

					str = str.Replace("\\\"u","ü");

					str = str.Replace("\\'y","ý");
					str = str.Replace("\\\"y","ÿ");
				}

				// Replace punctuation

				str = str.Replace("``","“");
				str = str.Replace("\'\'","”");
				str = str.Replace("`","‘");
				str = str.Replace("\'","’");

				str = str.Replace("--","—");
				str = str.Replace("-","–");
			}
			return str;
		}

	}
}
