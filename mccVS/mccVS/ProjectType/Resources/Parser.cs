using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace ProjectType.Resources
{
    class Parser
    {
        public static List<Completion> GetCompletions(SnapshotPoint snapshotPoint)
        {
            List<Completion> completions = new List<Completion>();

            string text = snapshotPoint.Snapshot.GetText(0, snapshotPoint.Position+1).Replace("\n/", "\n//").Replace("\nc/", "\n/c/").Split(new[] { "\n/" }, StringSplitOptions.RemoveEmptyEntries).Last();
            
            List<Token> tokens = new List<Token>();

            TokenType currentToken = TokenType.Nothing;

            List<Argument> arguments = new List<Argument>();
            foreach (string s in text.Split(new[] {' '}, StringSplitOptions.None))
            {
                if (currentToken == TokenType.Nothing && (text[0] == '/' || (text[0] == 'c' && text[1] == '/')))
                {
                    currentToken = TokenType.Command;
                    completions.Clear();
                    Commands.CommandList.ForEach(
                        cmd =>
                            completions.Add(new Completion(cmd.Name, cmd.Name, cmd.Documentation, cmd.Icon, null)));
                }
                else if (currentToken == TokenType.Command || currentToken == TokenType.Argument)
                {
                    currentToken = TokenType.Argument;
                    completions.Clear();
                    List<Command> commands =
                        Commands.CommandList.FindAll(
                            cmd => cmd.Name == tokens.Last(token => token.TokenType == TokenType.Command).Text);
                    if (commands.Count > 0)
                    {
                        Command command = commands.First();
                        int index = tokens.Count -
                                    tokens.FindLastIndex(token => token.TokenType == TokenType.Command) - 1;
                        Possibility[] possibilities = new Possibility[0];

                        arguments.Clear();
                        foreach (Argument currentArgument in command.Arguments)
                        {
                            if (currentArgument.NeededIndex < 0)
                                arguments.Add(currentArgument);
                            else if (currentArgument.NeededIndex < index &&
                                     currentArgument.NeededText ==
                                     (((tokens.FindLastIndex(token => token.TokenType == TokenType.Command) + 1 +
                                        currentArgument.NeededIndex) < tokens.Count)
                                         ? tokens[
                                             tokens.FindLastIndex(token => token.TokenType == TokenType.Command) + 1 +
                                             currentArgument.NeededIndex].Text
                                         : s))
                            {
                                arguments.Add(currentArgument);
                            }
                        }
                        if (index < arguments.Count)
                            possibilities = arguments[index].Possibilities;
                        foreach (Possibility possibility in possibilities)
                        {
                            if (possibility.Text == "|command|" && !possibilities.Select(p => p.Text).Contains(s))
                            {
                                Commands.CommandList.ForEach(
                                    cmd =>
                                        completions.Add(new Completion(cmd.Name, cmd.Name, cmd.Documentation,
                                            cmd.Icon, null)));
                                currentToken = TokenType.Command;
                            }
                            else
                                completions.Add(new Completion(possibility.Text, possibility.Text,
                                    possibility.Documentation, possibility.Icon, null));
                        }
                    }
                }

                string tokenText = s;
                if (currentToken == TokenType.Command && tokenText.Length > 0 && tokenText[0] == 'c')
                    tokenText = tokenText.Substring(1);
                tokens.Add(new Token(currentToken, tokenText));

            }
            return completions;
        }
    }

    class Token
    {
        public TokenType TokenType;
        public string Text;

        public Token(TokenType tokenType, string text)
        {
            TokenType = tokenType;
            Text = text;
        }
    }
    public enum TokenType
    {
        Nothing,
        Command,
        Argument,
    }
}
