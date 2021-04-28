
using System.Collections.Generic;
using System.Linq;

namespace MatrixSolver.Computations.DataTypes.Automata
{
    public class EquivalenceFinder
    {
        public Dictionary<int, Block> BlockLookup { get; }
        /// <summary>
        /// All known Blocks.
        /// </summary>
        public List<Block> Blocks { get; }
        public Automaton Automaton { get; }

        public EquivalenceFinder(Automaton automaton)
        {
            BlockLookup = new Dictionary<int, Block>();
            Blocks = new List<Block>();
            var finalStateBlock = new LinkedList<int>();
            var nonfinalStateBlock = new LinkedList<int>();
            Automaton = automaton;

            nonfinalStateBlock.AddLast(-1);
            foreach (var state in automaton.States)
            {
                if (automaton.FinalStates.Contains(state))
                {
                    finalStateBlock.AddLast(state);
                }
                else
                {
                    nonfinalStateBlock.AddLast(state);
                }
            }
            foreach (var stateSet in new[] { nonfinalStateBlock, finalStateBlock })
            {
                if (stateSet.Count == 0)
                {
                    continue;
                }
                var stateSetEquivalence = new Block(stateSet, this);
                foreach (var state in stateSet)
                {
                    BlockLookup[state] = stateSetEquivalence;
                }
                Blocks.Add(stateSetEquivalence);
            }
        }

        public void SeperateBlocksIntoEquivalences()
        {
            bool changes = true;
            while (changes)
            {
                changes = false;
                var blocksLength = Blocks.Count;
                for (int i = 0; i < blocksLength; i++)
                {
                    changes |= Blocks[i].UpdateBlocks();
                }
            }
        }
    }

    public class Block
    {
        public Block?[] Equivalence => CalculateEquivalenceForState(States.First!.Value);
        public LinkedList<int> States { get; }
        private readonly EquivalenceFinder _root;
        private Automaton _automaton => _root.Automaton;

        public Block(LinkedList<int> states, EquivalenceFinder root)
        {
            States = states;
            _root = root;
        }

        public bool UpdateBlocks()
        {
            bool changes = false;
            var newBlocks = new List<Block>();
            var startState = States.First!;
            var state = States.Last!;
            var blockUpdates = new List<(int state, Block block)>();
            var compareEquivalence = Equivalence;
            while (state != startState)
            {
                var nextState = state.Previous!;
                var stateEquivalence = CalculateEquivalenceForState(state.Value);

                if (!compareEquivalence.SequenceEqual(stateEquivalence))
                {
                    // Different equivalence, split them off into a new block, or check whether they fit into an existing one
                    changes = true;
                    States.Remove(state);

                    bool equivalenceFound = false;

                    foreach (var block in newBlocks)
                    {
                        if (block.Equivalence.SequenceEqual(stateEquivalence))
                        {
                            // Match found, remove the state from this equivalence, and add it into the block
                            block.AddState(state.Value);
                            blockUpdates.Add((state.Value, block));
                            equivalenceFound = true;
                            break;
                        }
                    }
                    if (!equivalenceFound)
                    {
                        // No equivalence found, create a new block
                        var newList = new LinkedList<int>();
                        newList.AddLast(state.Value);
                        var newBlock = new Block(newList, _root);
                        newBlocks.Add(newBlock);
                        blockUpdates.Add((state.Value, newBlock));
                        _root.Blocks.Add(newBlock);
                    }
                }
                state = nextState;
            }
            // Update the internal structure with the new block data
            foreach (var update in blockUpdates)
            {
                _root.BlockLookup[update.state] = update.block;
            }
            return changes;
        }

        private void AddState(int state)
        {
            States.AddLast(state);
        }

        private Block?[] CalculateEquivalenceForState(int state)
        {
            var previousBlocks = new Block?[_automaton.Alphabet.Count];

            for (int i = 0; i < _automaton.Alphabet.Count; i++)
            {
                if (state == -1)
                {
                    previousBlocks[i] = _root.BlockLookup[-1];
                    continue;
                }
                var states = _automaton.TransitionMatrix.GetStates(state, _automaton.Alphabet[i]);
                if (states.Count == 0)
                {
                    previousBlocks[i] = _root.BlockLookup[-1];
                }
                else
                {
                    previousBlocks[i] = _root.BlockLookup[states.First()];
                }
            }
            return previousBlocks;
        }
    }
}