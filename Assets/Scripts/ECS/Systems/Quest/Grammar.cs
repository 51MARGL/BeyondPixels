using System.Linq;
using System.Text.RegularExpressions;
using Unity.Mathematics;

namespace BeyondPixels.ECS.Systems.Quest
{
    public class Grammar
    {
        /// <summary>
        /// Terminal must be in form {YourTerminalName}
        /// </summary>
        public string[] TerminalAlphabet;
        public string StartTerminal;
        public Rule[] Rules;

        public string GenerateRandomText()
        {
            return ProductString(this.StartTerminal);
        }

        public string GenerateRandomText(string start)
        {
            return ProductString(start);
        }

        private string ProductString(string start)
        {
            var random = new Random((uint)System.Guid.NewGuid().GetHashCode());

            while (this.TerminalAlphabet.Any(t => start.Contains(t)))
            {
                var terminal = Regex.Match(start, "{[^{}]+}").Value;
                var rule = this.Rules.FirstOrDefault(r => r.LeftSide == terminal);
                if (rule == null)
                {
                    throw new System.Exception("Rule not found for:" + terminal + "\nstring:" + start);
                }
                if (rule.LeftSide.Contains("-"))
                {
                    var range = rule.LeftSide.Substring(1, rule.LeftSide.Length - 2)
                        .Split('-').Select(r => int.Parse(r)).ToArray();

                    var number = random.NextInt(range[0], range[1]);
                    start = start.Replace(terminal, "[" + number.ToString() + "]");
                }
                else
                {
                    start = start.Replace(terminal, rule.Products[random.NextInt(0, rule.Products.Length)]);
                }
            }

            return start;
        }
    }
}
