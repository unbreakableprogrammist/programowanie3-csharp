using System.Text;

namespace tasks;

/// <summary>
/// Here you should implement PascalToSnakeCase and SnakeToPascalCase string extension methods.
/// </summary>
public static class StringExtensions
{
   public static string PascalToSnakeCase(this string str)
   {
      if (string.IsNullOrEmpty(str))
      {
         return string.Empty;
      }

      string x = str[0].ToString().ToLower();
      for (int i = 1; i < str.Length;i++) // zaczymmy od jeden zeby nie patrzec na ta pierwsza literke
      {
         if (char.IsUpper(str[i]))
         {
            x += '_';
            x+=char.ToLower(str[i]);
         }
         else
         {
            x += str[i];
         }
      }

      return x;
   }

   public static string SnakeToPascalCase(this string str)
   {
      if (string.IsNullOrEmpty(str))
      {
         return string.Empty;
      }

      var sb = new StringBuilder();
      sb.Append(char.ToUpper(str[0]));
      for (int i = 1; i < str.Length; i++)
      {
         if (str[i] == '_')
         {
            sb.Append(char.ToUpper(str[(i + 1)]));
            i += 1;
         }
         else
            sb.Append(str[i]);
      }
      return sb.ToString();
   }
}