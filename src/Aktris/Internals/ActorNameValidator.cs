﻿using System.Text.RegularExpressions;
using Aktris.Exceptions;

namespace Aktris.Internals
{
	public static class ActorNameValidator
	{
		private const string _NameExtraCharacter = @"-_=+,.!~";
		private const string _AlphaNum = @"a-zA-Z0-9";
		private const string _Pattern = @"^[" + _AlphaNum + @"]([" + _AlphaNum + _NameExtraCharacter + @"])*$";
		private static readonly Regex _ValidNameRegex = new Regex(_Pattern, RegexOptions.Compiled);


		public static void EnsureNameIsValid(string name)
		{
			if(string.IsNullOrEmpty(name)) throw new InvalidActorNameException("The name may not be empty string.");
			if(!_ValidNameRegex.IsMatch(name)) throw new InvalidActorNameException(string.Format("Invalid name \"{1}\". The name must start with alpha-numerical (a-zAZ-09) then followed by alphanumerical including the characters {0}", _NameExtraCharacter, name));
		}


	}
}