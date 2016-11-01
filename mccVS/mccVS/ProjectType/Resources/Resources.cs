using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;

namespace ProjectType.Resources
{
    public class Command
    {
        public string Name;
        public string Documentation;
        public Argument[] Arguments;
        public ImageSource Icon;

        public Command(string name, string documentation, Argument[] arguments, string icon)
        {
            Name = name;
            Documentation = documentation;
            Arguments = arguments;
            Icon = icon == null ? null : new BitmapImage(new Uri(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\CustomProjectSystems\\mccVS\\Resources\\assets\\minecraft\\textures\\" + icon));
            if (Icon != null)
                Icon = new CroppedBitmap((BitmapSource)Icon,
                new Int32Rect(0, 0, (int)(Icon.Height < Icon.Width ? Icon.Height : Icon.Width),
                    (int)(Icon.Height < Icon.Width ? Icon.Height : Icon.Width)));
        }
    }
    public class Commands
    {
        public static List<Command> CommandList = new List<Command>();
        public static List<string> CommandTextList = new List<string>();

        public Commands()
        {
            CommandList = new List<Command>();
            CommandTextList = new List<string>();

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\CustomProjectSystems\\mcc\\Resources\\resources.xml");

            foreach (XmlNode command in xmlDoc["Resources"].ChildNodes[0].ChildNodes)//["commands"].ChildNodes)
            {
                CommandTextList.Add('/' + command.Name);
                List<Argument> arguments = new List<Argument>();
                foreach (XmlNode argument in command.ChildNodes)
                {
                    List<Possibility> possibilities = new List<Possibility>();
                    foreach (XmlNode possibility in argument.ChildNodes)
                    {
                        if (possibility.Attributes["name"].Value == "|achievement|")
                            foreach (XmlNode childNode in xmlDoc["Resources"].ChildNodes[1].ChildNodes)
                            {
                                possibilities.Add(new Possibility(childNode.Attributes["name"].Value, childNode.Attributes["doc"]?.Value, childNode.Attributes["img"]?.Value));
                            }
                        else if (possibility.Attributes["name"].Value == "|block|")
                            foreach (XmlNode childNode in xmlDoc["Resources"].ChildNodes[2].ChildNodes)
                            {
                                possibilities.Add(new Possibility(childNode.Attributes["name"].Value, childNode.Attributes["doc"]?.Value, childNode.Attributes["img"]?.Value));
                            }
                        else
                            possibilities.Add(new Possibility(possibility.Attributes["name"].Value, possibility.Attributes["doc"]?.Value, possibility.Attributes["img"]?.Value));

                    }
                    if (argument.Attributes["needs"] != null)
                        arguments.Add(new Argument(argument.Attributes["name"].Value, argument.Attributes["doc"]?.Value, possibilities.ToArray(), Convert.ToInt32(argument.Attributes["needs"]?.Value.Split('-').First()), argument.Attributes["needs"]?.Value.Split('-').Last()));
                    else
                    {
                        arguments.Add(new Argument(argument.Attributes["name"].Value, argument.Attributes["doc"]?.Value, possibilities.ToArray(), -1, ""));
                    }
                }
                CommandList.Add(new Command('/' + command.Name, command.Attributes["doc"].Value, arguments.ToArray(), command.Attributes["img"]?.Value));
            }
        }
    }

    public class Argument
    {
        public string Name;
        public string Documentation;
        public Possibility[] Possibilities;
        public int NeededIndex;
        public string NeededText;

        public Argument(string name, string documentation, Possibility[] possibilities, int neededIndex, string neededText)
        {
            Name = name;
            Documentation = documentation;
            Possibilities = possibilities;
            NeededIndex = neededIndex;
            NeededText = neededText;
        }
    }
    public class Possibility
    {
        public string Text;
        public string Documentation;
        public ImageSource Icon;

        public Possibility(string text, string documentation, string icon)
        {
            Text = text;
            Documentation = documentation;
            Icon = icon == null ? null : new BitmapImage(new Uri(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\CustomProjectSystems\\mccVS\\Resources\\assets\\minecraft\\textures\\" + icon));
        }
    }

}
