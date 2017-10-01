// Copyright(c) 2017 Takahiro Miyaura
// Released under the MIT license
// http://opensource.org/licenses/mit-license.php
using System;
using Windows.ApplicationModel.Core;
using Urho;

namespace UrhnoBlockStackingSamples
{
    internal class Program
    {
        [MTAThread]
        private static void Main()
        {
            var appViewSource = new UrhoAppViewSource<BlockStackingSamples>();
            CoreApplication.Run(appViewSource);
        }
    }
}