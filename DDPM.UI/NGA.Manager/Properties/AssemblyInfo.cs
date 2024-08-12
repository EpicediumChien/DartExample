#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

//[assembly: System.Reflection.AssemblyProduct("MyDell")]
//This assembly is targeted to Windows Platform
#if NET
[assembly: SupportedOSPlatform("windows")]
#endif
//[assembly: System.Reflection.AssemblyTitle("MyDell Notification Manager")]

// Use the following VS command line utility to get the key: sn -Tp <assemblyPath>
[assembly: InternalsVisibleTo("NGA.Manager.Tests, PublicKey=" +
          "0024000004800000940000000602000000240000525341310004000001000100251f2adb2e977e" +
          "ab05a276c87c32434d1755001e376ce90c6747da00de750980402d2c66d8e5801f844318379312" +
          "2b5e6f48e0c7eb6917752de65f1abcbeaeafbc233eff8b63620270b1885d80da016f3c2eaf7c6f" +
          "2ec318ce3256101f74e4a5812efb06adbb5f648599b2b4f4dbe0eedd75d921d270560b743cf8337f2244c3")]

// This is for Moq
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=" +
          "0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99" +
          "c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654" +
          "753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46" +
          "ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
