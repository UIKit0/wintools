﻿//
// Authors:
// Jonathan Pobst (monkey@jpobst.com)
//
// Copyright (C) 2009 Jonathan Pobst 
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Xml;

namespace MonkeyBuilder.MonoCompiler
{
	public class SerialWorkQueue
	{
		private Queue<XmlElement> queue = new Queue<XmlElement> ();

		public SerialWorkQueue (XmlDocument doc)
		{
			foreach (XmlNode node in doc.DocumentElement.ChildNodes) {
				if (!(node is XmlElement))
					continue;

				XmlElement xe = (XmlElement)node;

				if (xe.GetAttribute ("enabled") == "false")
					continue;
				if (xe.GetAttribute ("installeronly") == "true")
					continue;
					
				queue.Enqueue (xe);
			}
		}
		
		// Return value: true if have work, false if done
		public bool GetWork (out XmlElement work)
		{
			work = null;
			
			if (queue.Count == 0)
				return false;
				
			work = queue.Dequeue ();
			return true;
		}


		public void ReportWorkCompleted (XmlElement xe)
		{
		}
	}
}
