namespace Santase.AI.ProPlayer.Tools.Extensions
{
	using System;
	using System.Collections.Generic;

	public static class ExtensionsIEnumerable
	{
		public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
		{
			foreach (var item in source)
			{
				action(item);
			}

			return source;
		}
	}
}