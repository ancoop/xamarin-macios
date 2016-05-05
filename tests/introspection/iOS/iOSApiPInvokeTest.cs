﻿//
// Test the existing of p/invoked symbols
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2014-2015 Xamarin Inc. All rights reserved.
//

using System;
using System.Reflection;
#if XAMCORE_2_0
using Foundation;
using ObjCRuntime;
using UIKit;
#else
using MonoTouch;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
#endif
using NUnit.Framework;

namespace Introspection {

	[TestFixture]
	// we want the tests to be available because we use the linker
	[Preserve (AllMembers = true)]
	public class iOSApiPInvokeTest : ApiPInvokeTest {

		protected override bool Skip (string symbolName)
		{
			bool simulator = Runtime.Arch == Arch.SIMULATOR;
			switch (symbolName) {
			// Metal support inside simulator is only available in recent iOS9 SDK
#if !__WATCHOS__
			case "MTLCreateSystemDefaultDevice":
				return simulator && !UIDevice.CurrentDevice.CheckSystemVersion (9, 0);
#endif
			// still most Metal helpers are not available on the simulator (even when the framework is present, it's missing symbols)
			case "MPSSupportsMTLDevice":
			// neither are the CoreVideo extensions for Metal
			case "CVMetalTextureGetTexture":
			case "CVMetalTextureIsFlipped":
			case "CVMetalTextureGetCleanTexCoords":
			case "CVMetalTextureCacheCreate":
			case "CVMetalTextureCacheFlush":
			case "CVMetalTextureCacheCreateTextureFromImage":
			case "MTKMetalVertexDescriptorFromModelIO":
			case "MTKModelIOVertexDescriptorFromMetal":
			case "MTKModelIOVertexFormatFromMetal":
			case "MTKMetalVertexFormatFromModelIO":
				return simulator;

			// it's not needed for ARM64 and Apple does not have stubs for them in libobjc.dylib
			case "objc_msgSend_stret":
			case "objc_msgSendSuper_stret":
				return IntPtr.Size == 8 && !simulator;

			default:
				return base.Skip (symbolName);
			}
		}

		protected override bool SkipAssembly (Assembly a)
		{
			// we only want to check this on a version of iOS that
			// 1. is the current SDK target (or a newer one)
#if !__WATCHOS__
			var sdk = new Version (Constants.SdkVersion);
			if (!UIDevice.CurrentDevice.CheckSystemVersion (sdk.Major, sdk.Minor))
				return true;
#endif
			// 2. on the real target for Xamarin.iOS.dll/monotouch.dll
			//    as the simulator miss some libraries and symbols
			//    but the rest of the BCL is fine to test
			return (a == typeof (NSObject).Assembly && (Runtime.Arch == Arch.SIMULATOR));
		}

		[Test]
		public void NUnitLite ()
		{
			var a = typeof (TestAttribute).Assembly;
			if (!SkipAssembly (a))
				Check (a);
		}

#if !__WATCHOS__
		[Test]
		public void MonoTouchDialog ()
		{
			// there's no direct reference to MTD - but it's there
			var a = AppDelegate.Runner.NavigationController.TopViewController.GetType ().Assembly;
			if (!SkipAssembly (a))
				Check (a);
		}
#endif
	}
}