using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using MatrixSolver.Computations.DataTypes.Automata;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.WpfGraphControl;
using Color = Microsoft.Msagl.Drawing.Color;
using ModifierKeys = System.Windows.Input.ModifierKeys;

namespace MatrixSolver
{
    class MainWindow : Window
    {
        Grid mainGrid = new Grid();
        DockPanel graphViewerPanel = new DockPanel();
        GraphViewer graphViewer = new GraphViewer();
        TextBox statusTextBox;

        private readonly Automaton _automaton;

        public MainWindow(Automaton automaton)
        {
            _automaton = automaton ?? throw new ArgumentNullException(nameof(automaton));
            Title = "MainWindow";
            Content = mainGrid;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowState = WindowState.Normal;

            graphViewerPanel.ClipToBounds = true;
            graphViewer.ObjectUnderMouseCursorChanged += graphViewer_ObjectUnderMouseCursorChanged;

            mainGrid.Children.Add(graphViewerPanel);
            graphViewer.BindToPanel(graphViewerPanel);

            SetStatusBar();
            graphViewer.MouseDown += MainWindow_MouseDown;
            Loaded += (a, b) => CreateAndLayoutAndDisplayGraph(null, null);

        }

        void MainWindow_MouseDown(object sender, MsaglMouseEventArgs e)
        {
            statusTextBox.Text = "there was a click...";
        }

        void SetStatusBar()
        {
            var statusBar = new StatusBar();
            statusTextBox = new TextBox { Text = "No object" };
            statusBar.Items.Add(statusTextBox);
            mainGrid.Children.Add(statusBar);
            statusBar.VerticalAlignment = VerticalAlignment.Bottom;
        }

        void graphViewer_ObjectUnderMouseCursorChanged(object sender, ObjectUnderMouseCursorChangedEventArgs e)
        {
            var node = graphViewer.ObjectUnderMouseCursor as IViewerNode;
            if (node != null)
            {
                var drawingNode = (Node)node.DrawingObject;
                statusTextBox.Text = drawingNode.Label.Text;
            }
            else
            {
                var edge = graphViewer.ObjectUnderMouseCursor as IViewerEdge;
                if (edge != null)
                    statusTextBox.Text = ((Edge)edge.DrawingObject).SourceNode.Label.Text + "->" +
                                         ((Edge)edge.DrawingObject).TargetNode.Label.Text;
                else
                    statusTextBox.Text = "No object";
            }
        }


        private void CreateAndLayoutAndDisplayGraph(object sender, ExecutedRoutedEventArgs ex)
        {
            var alphabetAndEpsilon = _automaton.Alphabet.Append(Automaton.Epsilon);
            try
            {
                Graph graph = new Graph();

                var vertexLookup = new Dictionary<int, string>();
                // Add states
                foreach (var state in _automaton.States)
                {
                    var stateId = $"S{state}";
                    var node = new Node(stateId);
                    if (_automaton.StartStates.Contains(state))
                    {
                        node.Attr.Color = Color.Red;
                        node.Attr.Styles.Append(Microsoft.Msagl.Drawing.Style.Dashed);
                    }
                    if (_automaton.FinalStates.Contains(state))
                    {
                        node.Attr.Shape = Shape.DoubleCircle;
                    }
                    else
                    {
                        node.Attr.Shape = Shape.Circle;
                    }

                    vertexLookup[state] = stateId;
                    graph.AddNode(node);
                }
                // Add transitions
                foreach (var state in _automaton.States)
                {
                    var reachableStatesLookup = new Dictionary<int, List<char>>();
                    foreach (var symbol in alphabetAndEpsilon)
                    {
                        var reachableStates = _automaton.TransitionMatrix.GetStates(state, symbol);
                        // Should be 1 or 0 states, but for flexibility, we handle any number
                        foreach (var reachableState in reachableStates)
                        {
                            if(!reachableStatesLookup.TryGetValue(reachableState, out var chars))
                            {
                                chars = new List<char>(_automaton.Alphabet.Count);
                                reachableStatesLookup.Add(reachableState, chars);
                            }
                            chars.Add(symbol);
                        }
                    }
                    foreach(var (reachableState, symbols) in reachableStatesLookup)
                    {
                        graph.AddEdge(vertexLookup[state], String.Join(',', symbols), vertexLookup[reachableState]);
                    }
                }

                graph.Attr.LayerDirection = LayerDirection.LR;
                graphViewer.Graph = graph;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Load Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
