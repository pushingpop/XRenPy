﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace X_Ren_Py
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public class XFrame : ListViewItem
		{
			//string _Content;
			private string _Text = "";
			private ObservableCollection<XMenuOption> _MenuOptions;
			private XCharacter _Character;
			private XMovie _Movie;

			public string Text { get { return _Text; } set { _Text = value; } }
			public ObservableCollection<XMenuOption> MenuOptions { get { return _MenuOptions; } set { _MenuOptions = value; } }
			public XCharacter Character { get { return _Character; } set { _Character = value; } }
			public XMovie Movie { get { return _Movie; } set { _Movie = value; } }

			public XFrame()
			{
				Content = "[]"; 
				ContextMenu = cmFrame;
			}			
		}
		private XFrame createFrame()
		{
			XFrame frame = new XFrame() { Character = characterListView.Items[0] as XCharacter};
			frame.Selected += selectFrame_Click;
			frame.MouseUp += selectFrame_Click;
			frame.Tag = tabControlStruct.SelectedItem;
			return frame;
		}

		private void selectFrame_Click(object sender, RoutedEventArgs e)
		{
			uncheckAll();
			addorselect = false;
			currentFrame = sender as XFrame;
			setPreviousFrames();

			characterListView.SelectedItem = currentFrame.Character;
			textBox.Text = currentFrame.Text;
			showCharacter();

			if (currentFrame.MenuOptions == null)
			{
				menuStack.Visibility = Visibility.Hidden;
				convertFrameMenu.Header = framemenu;
				menuOptionsVisualList.ItemsSource = null;
			}
			else
			{
				menuStack.Visibility = Visibility.Visible;
				convertFrameMenu.Header = menuframe;
				menuOptionsVisualList.ItemsSource = currentFrame.MenuOptions;
			}

			//ресурсы			
			//при выборе фрейма сначала проверяется, есть ли пропы ТОЛЬКО предыдущих кадров включая нынешний, откидывается полностью часть пропов со стоп-маркерами в виде предыдущих же кадров
			//проще говоря, в списке оказываются только те пропы, у которых есть начало, но нет конца до нынешнего фрейма включительно
			List<ImageBackProperties> backgroundslist = BackInFrameProps.Where(back => previousFrames.Contains(back.Frame) && ((back.StopFrame == null && back.StopFrames==null) || back.StopFrames.Intersect(previousFrames).Count()==0)).ToList();
			List<ImageCharProperties> imageslist = ImageInFrameProps.Where(img => previousFrames.Contains(img.Frame) && ((img.StopFrame == null && img.StopFrames == null) || img.StopFrames.Intersect(previousFrames).Count() == 0)).ToList();
			List<AudioProperties> audiolist = AudioInFrameProps.Where(mus => previousFrames.Contains(mus.Frame) && ((mus.StopFrame == null && mus.StopFrames == null) || mus.StopFrames.Intersect(previousFrames).Count() == 0)).ToList();

			//пропов будет всегда немного, потому по ним искать легче легкого и проще простого.
			foreach (ImageBackProperties backprops in backgroundslist)
			{
				if (backprops.Frame != currentFrame) backprops.Image.IsChecked = null; else backprops.Image.IsChecked = true;
				backprops.Image.Background = currentFrameResourceColor;
			}

			foreach (ImageCharProperties imageprops in imageslist)
			{
				if (imageprops.Frame != currentFrame) imageprops.Image.IsChecked = null; else imageprops.Image.IsChecked = true;
				imageprops.Image.Background = currentFrameResourceColor;
			}
			foreach (AudioProperties audprops in audiolist)
			{
				if (audprops.Frame != currentFrame) audprops.Audio.IsChecked = null; else audprops.Audio.IsChecked = true;
				audprops.Audio.Background = currentFrameResourceColor;
			}

			addorselect = true;
		}

		private void addNextFrame_Click(object sender, RoutedEventArgs e)
		{
			XFrame frame = createFrame();
			ListView selectedList = getSelectedList();

			if (sender == addMenu)
			{
				frame.MenuOptions = new ObservableCollection<XMenuOption> { createMenuOption(true) };
			}

			selectedList.Items.Insert(selectedList.Items.IndexOf(selectedList.SelectedItem) + 1, frame);
			frame.IsSelected = true;
		}
		
		private void deleteFrame_Click(object sender, EventArgs e) { if(getSelectedList().Items.Count>1) getSelectedList().Items.Remove(getSelectedFrame()); else MessageBox.Show("Error: Label must contain at least one frame", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }

		private void PrevNext_Click(object sender, RoutedEventArgs e)
		{			
			int index = getSelectedList().Items.IndexOf(getSelectedFrame());
			if (sender == prevFrame && index - 1 >= 0) (getSelectedList().Items[index - 1] as XFrame).IsSelected = true;
			else if (sender == nextFrame && index + 1 < getSelectedList().Items.Count) (getSelectedList().Items[index + 1] as XFrame).IsSelected = true;
		}

		private void textBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			currentFrame.Text = textBox.Text;
			currentFrame.Content = "[" + textBox.Text + ']';
		}

		private XFrame getSelectedFrame() { return getSelectedList().SelectedItem as XFrame; }
		private ListView getSelectedList() { return tabControlStruct.SelectedContent as ListView; }

		private void setPreviousFrames()
		{
			previousFrames.Clear();
			getPreviousFrames();
			previousFrames.Reverse();
		}

		private void getPreviousFrames()
		{   			
			XFrame firstframe = currentFrame;
				for (int i = (currentFrame.Parent as ListView).Items.IndexOf(currentFrame); i >=0; i--) previousFrames.Add((currentFrame.Parent as ListView).Items[i] as XFrame);
			if ((currentFrame.Tag as XLabel).MenuChoice!=null&&(currentFrame.Tag as XLabel).Text != "start")
			{ currentFrame = (currentFrame.Tag as XLabel).MenuChoice;
				getPreviousFrames(); }
			currentFrame = firstframe;		 
		}
	}
}