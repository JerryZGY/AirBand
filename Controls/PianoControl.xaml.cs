﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Sanford.Multimedia.Midi;

namespace KinectAirBand.Controls
{
    public partial class PianoControl : UserControl
    {
        private const int LowNoteID = 41;
        private const int HighNoteID = 89;
        private delegate void NoteMessageCallback(ChannelMessage message);
        private NoteMessageCallback noteOnCallback;
        private NoteMessageCallback noteOffCallback;
        SynchronizationContext context;
        List<PianoKey> keys = new List<PianoKey>();
        int whiteKeyCount = 0;

        public enum KeyType
        {
            White,
            Black
        }

        private static readonly KeyType[] KeyTypeTable =
        {
            KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White, KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White,
            KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White, KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White,
            KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White, KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White,
            KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White, KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White,
            KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White, KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White,
            KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White, KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White,
            KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White, KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White,
            KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White, KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White,
            KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White, KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White,
            KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White, KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White,
            KeyType.White, KeyType.Black, KeyType.White, KeyType.Black, KeyType.White, KeyType.White, KeyType.Black, KeyType.White
        };

        private static readonly int[] Ids =
        {
            0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0,
            0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0,
            0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0,
            0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0,
            0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0,
            0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0,
            0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0,
            0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0,
            0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0,
            0, 1, 0, 1, 0, 0, 1, 0, 1, 0, 1, 0,
            0, 1, 0, 1, 0, 0, 1, 0
        };

        public PianoControl ()
        {
            InitializeComponent();
            CreatePianoKeys();
            context = SynchronizationContext.Current;
            noteOnCallback = delegate(ChannelMessage message)
            {
                if (message.Data2 > 0)
                {
                    keys[message.Data1 - LowNoteID].PressPianoKey();
                }
                else
                {
                    keys[message.Data1 - LowNoteID].ReleasePianoKey();
                }
            };

            noteOffCallback = delegate(ChannelMessage message)
            {
                keys[message.Data1 - LowNoteID].ReleasePianoKey();
            };
        }

        private void CreatePianoKeys()
        {
            whiteKeyCount = 0;
            double nextLeft = 0;
            for (int i = 0; i < HighNoteID - LowNoteID; i++)
            {
                var key = new PianoKey(KeyTypeTable[i + LowNoteID]);
                key.NoteID = i + LowNoteID;
                if (KeyTypeTable[key.NoteID] == KeyType.White)
                {
                    key.NoteOffColor = Colors.White;
                    whiteKeyCount++;
                    key.Width = 47;
                    key.Height = 191;
                    key.Margin = new Thickness(nextLeft, 0, 0, 0);
                    nextLeft += 47;
                    key.SetValue(Canvas.ZIndexProperty, 0);
                }
                else
                {
                    key.NoteOffColor = Colors.Black;
                    key.Width = 29.375;
                    key.Height = 119.375;
                    key.Margin = new Thickness(nextLeft - 14.6875, 0, 0, 0);
                    key.SetValue(Canvas.ZIndexProperty, 10);
                }
                keys.Add(key);
                cnvPiano.Children.Add(key);
            }
        }

        public void Send(ChannelMessage message)
        {
            if (message.Command == ChannelCommand.NoteOn &&
                message.Data1 >= LowNoteID && message.Data1 <= HighNoteID)
            {
                noteOnCallback(message);
            }
            else if (message.Command == ChannelCommand.NoteOff &&
                message.Data1 >= LowNoteID && message.Data1 <= HighNoteID)
            {
                noteOffCallback(message);
            }
        }

        public void Clear()
        {
            for (var i = LowNoteID; i < HighNoteID; i++)
            {
                keys[i - LowNoteID].ReleasePianoKey();
            }
        }
    }
}
