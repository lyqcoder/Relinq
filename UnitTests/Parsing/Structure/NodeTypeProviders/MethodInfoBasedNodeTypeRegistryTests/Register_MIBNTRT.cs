﻿// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;
using Remotion.Linq.UnitTests.Parsing.Structure.NodeTypeProviders.MethodInfoBasedNodeTypeRegistryTests.TestDomain;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Parsing.Structure.NodeTypeProviders.MethodInfoBasedNodeTypeRegistryTests
{
  [TestFixture]
  public class Register_MethodInfoBasedNodeTypeRegistryTest
  {
    private MethodInfoBasedNodeTypeRegistry _registry;

    [SetUp]
    public void SetUp ()
    {
      _registry = new MethodInfoBasedNodeTypeRegistry();
    }

    [Test]
    public void Test_WithMethodInfo ()
    {
      Assert.That (_registry.RegisteredMethodInfoCount, Is.EqualTo (0));

      _registry.Register (SelectExpressionNode.GetSupportedMethods(), typeof (SelectExpressionNode));

      Assert.That (_registry.RegisteredMethodInfoCount, Is.EqualTo (2));
    }

    [Test]
    public void Test_SameMethodTwice_OverridesPreviousNodeType ()
    {
      var registry = _registry;
      registry.Register (WhereExpressionNode.GetSupportedMethods(), typeof (SelectExpressionNode));
      registry.Register (WhereExpressionNode.GetSupportedMethods(), typeof (WhereExpressionNode));

      var type = registry.GetNodeType (WhereExpressionNode.GetSupportedMethods().First());
      Assert.That (type, Is.SameAs (typeof (WhereExpressionNode)));
    }

    [Test]
    public void Test_WithMethodInfoAndClosedGenericMethod_NotAllowed ()
    {
      var closedGenericMethod = ReflectionUtility.GetMethod (() => Queryable.Select (null, (Expression<Func<int, int>>) null));

      Assert.That (
          () => _registry.Register (new[] { closedGenericMethod }, typeof (SelectExpressionNode)),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "Cannot register closed generic method 'Select', try to register its generic method definition instead."));
    }

    [Test]
    public void Test_WithMethodInfoAndMethodInClosedGenericType_NotAllowed ()
    {
      var methodInClosedGenericType = typeof (GenericClass<int>).GetMethod ("NonGenericMethod");
      Assert.That (
          () => _registry.Register (new[] { methodInClosedGenericType }, typeof (SelectExpressionNode)),
          Throws.InvalidOperationException.With.Message.EqualTo (
              "Cannot register method 'NonGenericMethod' in closed generic type "
              + "'Remotion.Linq.UnitTests.Parsing.Structure.NodeTypeProviders.MethodInfoBasedNodeTypeRegistryTests.TestDomain.GenericClass`1[System.Int32]', "
              + "try to register its equivalent in the generic type definition instead."));
    }
  }
}