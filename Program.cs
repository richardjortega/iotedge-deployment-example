﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Samples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DeploySample deploySample = new DeploySample();
            deploySample.RunSampleAsync().GetAwaiter().GetResult();
        }
    }
}