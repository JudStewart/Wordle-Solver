using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace WordleSolver
{
	public partial class Form1 : Form
	{
		int currentLabel = 0;
		Label[] labels;
		Colors[] labelColors = new Colors[30];
		Slot[] slots = new Slot[] { new Slot(), new Slot(), new Slot(), new Slot(), new Slot() };
		List<char> allYellows = new List<char>();
		enum Colors { GRAY, GREEN, YELLOW, CLEAR}
		Color[] colors;

		string[] words;

		public Form1()
		{
			InitializeComponent();
			labels = new Label[] { label0, label1, label2, label3, label4, label5, label6, label7, label8, label9, label10, label11, label12, label13, label14, label15, label16, label17, label18, label19, label20, label21, label22, label23, label24, label25, label26, label27, label28, label29 };
			colors = new Color[] { Color.FromArgb(120, 124, 126), Color.FromArgb(106, 170, 100), Color.FromArgb(201, 180, 88), SystemColors.Control};
			for (int l = 0; l < 30; l++) labelColors[l] = Colors.CLEAR;
			parseWords();
		}

		private void parseWords()
		{
			var reader = new StreamReader("../../wordle.csv");
			var line = reader.ReadLine();
			var splits = line.Split(',');
			words = splits;
			Array.Sort(words);
		}

		private void Form1_KeyPress(object sender, KeyPressEventArgs e)
		{
			//MessageBox.Show("Current Label: " + currentLabel);
			if (char.IsLetter(e.KeyChar))
			{
				labels[currentLabel].Text = e.KeyChar.ToString().ToUpper();
				labelColors[currentLabel] = Colors.GRAY;
				labels[currentLabel].BackColor = colors[(int) Colors.GRAY];
				labels[currentLabel].ForeColor = Color.White;
				labelColors[currentLabel] = Colors.GRAY;
				if (currentLabel < 29) currentLabel++;
			}
			else if (e.KeyChar == (char)Keys.Back)
			{
				if (currentLabel > 0 && currentLabel < 29) currentLabel--;
				if (currentLabel == 29 && labels[currentLabel].Text == "") currentLabel--;
				labels[currentLabel].Text = "";
				labels[currentLabel].BackColor = colors[(int)Colors.CLEAR];
				labels[currentLabel].ForeColor = Color.Black;
				labelColors[currentLabel] = Colors.CLEAR;
				//if (currentLabel == 29) currentLabel--;
			}
			else
			{
				//MessageBox.Show("Invalid Input");
				e.Handled = false;
			}
		}

		private void label_Click(object sender, EventArgs e)
		{
			Label label = (Label)sender;
			if (label.Text.Equals("")) return;
			int index = Array.IndexOf(labels, label);
			Colors newColor = (Colors) (((int)labelColors[index] + 1) % 3);
			//MessageBox.Show("New color value is " + Enum.GetName(typeof(Colors), newColor));
			label.BackColor = colors[(int) newColor];
			labelColors[index] = newColor;
		}

		private void solve(object sender, EventArgs e)
		{
			List<char> grayLetters = new List<char>();
			for (int l = 0; l < 30; l++)
			{
				switch (labelColors[l])
				{
					case Colors.CLEAR:
						break;
					case Colors.GRAY:
						grayLetters.Add(labels[l].Text[0]);
						break;
					case Colors.GREEN:
						slots[l % 5].mustBe = labels[l].Text[0];
						break;
					case Colors.YELLOW:
						addYellow(labels[l].Text[0], l % 5);
						break;
					default:
						break;
				}
			}

			List<string> filteredList = new List<string>();

			char[] grayArr = grayLetters.ToArray();
			char[] yellowArr = allYellows.ToArray();

			//MessageBox.Show("Gray Letters: " + (new string(grayArr)) + "\nYellow Letters: " + (new string(yellowArr)));
			
			foreach (string word in words)
			{
				if (containsNone(word, grayArr) && containsAll(word, yellowArr))
				{
					filteredList.Add(word);
				}
			}

			//MessageBox.Show("Count: " + filteredList.Count());

			string pattern = "";
			for (int l = 0; l < 5; l++)
			{
				string thisSlot = "";
				if (slots[l].mustBe != '0')
				{
					thisSlot += slots[l].mustBe;
				}
				else
				{
					if (slots[l].cantBe.Count == 0)
					{
						thisSlot = "[A-Z]";
					}
					else thisSlot = "[^" + new String(slots[l].cantBe.Distinct().ToArray()) + "]";
				}
				pattern += thisSlot;
			}

			//MessageBox.Show("Final regex pattern is " + pattern, "DEBUG");

			Regex rg = new Regex(pattern);
			var solutions = filteredList.Where<string>(word => rg.IsMatch(word));

			//if (solutions.Count() == 0) MessageBox.Show("Something is wrong, nothing came back.");
			listBox1.DataSource = solutions.ToArray();
		}

		private void addYellow(char c, int slot)
		{
			allYellows.Add(c);
			for (int l = 0; l < 5; l++)
			{
				if (l == slot)
				{
					slots[l].cantBe.Add(c);
				}
				else slots[l].couldBe.Add(c);
			}
		}

		private bool containsAll(string s, char[] arr)
		{
			foreach (char c in arr)
			{
				if (s.IndexOf(c) == -1) return false;
			}
			return true;
		}

		private bool containsNone(string s, char[] arr)
		{
			foreach (char c in arr)
			{
				if (s.IndexOf(c) != -1) return false;
			}
			return true;
		}
	}

	class Slot
	{
		public char mustBe;
		public List<char> couldBe;
		public List<char> cantBe;

		public Slot()
		{
			mustBe = '0';
			couldBe = new List<char>();
			cantBe = new List<char>();
		}
	}
}
