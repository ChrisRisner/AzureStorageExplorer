using System;
using System.Runtime.InteropServices;

namespace AzureStorageExplorer.iOS
{
	public static class NativeTools
	{
		[DllImport(ObjCRuntime.Constants.MobileCoreServicesLibrary, EntryPoint = "UTTypeCopyPreferredTagWithClass")]
		public extern static IntPtr UTTypeCopyPreferredTagWithClass(IntPtr uti, IntPtr tagClass);

		[DllImport(ObjCRuntime.Constants.MobileCoreServicesLibrary, EntryPoint = "UTTypeCreatePreferredIdentifierForTag")]
		public extern static IntPtr UTTypeCreatePreferredIdentifierForTag(IntPtr tagClass, IntPtr tag, IntPtr uti);
	}
}
