/*
 * Copyright 2010-2016 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 *
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using NanoByte.Common.Values;

// ReSharper disable All

namespace ZeroInstall.Publish.EntryPoints
{

    #region File header structures
    [StructLayout(LayoutKind.Sequential)]
    [CLSCompliant(false)]
    public struct ImageDosHeader
    {
        public UInt16 e_magic; // Magic number
        public UInt16 e_cblp; // Bytes on last page of file
        public UInt16 e_cp; // Pages in file
        public UInt16 e_crlc; // Relocations
        public UInt16 e_cparhdr; // Size of header in paragraphs
        public UInt16 e_minalloc; // Minimum extra paragraphs needed
        public UInt16 e_maxalloc; // Maximum extra paragraphs needed
        public UInt16 e_ss; // Initial (relative) SS value
        public UInt16 e_sp; // Initial SP value
        public UInt16 e_csum; // Checksum
        public UInt16 e_ip; // Initial IP value
        public UInt16 e_cs; // Initial (relative) CS value
        public UInt16 e_lfarlc; // File address of relocation table
        public UInt16 e_ovno; // Overlay number
        public UInt16 e_res_0; // Reserved words
        public UInt16 e_res_1; // Reserved words
        public UInt16 e_res_2; // Reserved words
        public UInt16 e_res_3; // Reserved words
        public UInt16 e_oemid; // OEM identifier (for e_oeminfo)
        public UInt16 e_oeminfo; // OEM information; e_oemid specific
        public UInt16 e_res2_0; // Reserved words
        public UInt16 e_res2_1; // Reserved words
        public UInt16 e_res2_2; // Reserved words
        public UInt16 e_res2_3; // Reserved words
        public UInt16 e_res2_4; // Reserved words
        public UInt16 e_res2_5; // Reserved words
        public UInt16 e_res2_6; // Reserved words
        public UInt16 e_res2_7; // Reserved words
        public UInt16 e_res2_8; // Reserved words
        public UInt16 e_res2_9; // Reserved words
        public UInt32 e_lfanew; // File address of new exe header
    }

    [StructLayout(LayoutKind.Sequential)]
    [CLSCompliant(false)]
    public struct ImageDataDirectory
    {
        public UInt32 VirtualAddress;
        public UInt32 Size;
    }

    [SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
    [CLSCompliant(false)]
    public enum MachineType : ushort
    {
        Native = 0,
        I386 = 0x014c,
        Itanium = 0x0200,
        X64 = 0x8664
    }

    [SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32"), SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    [CLSCompliant(false)]
    public enum Subsystem : ushort
    {
        Native = 1,
        WindowsGui = 2,
        WindowsCui = 3,
        OS2Cui = 5,
        PosixCui = 7
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [CLSCompliant(false)]
    public struct ImageOptionalHeader32
    {
        public UInt16 Magic;
        public Byte MajorLinkerVersion;
        public Byte MinorLinkerVersion;
        public UInt32 SizeOfCode;
        public UInt32 SizeOfInitializedData;
        public UInt32 SizeOfUninitializedData;
        public UInt32 AddressOfEntryPoint;
        public UInt32 BaseOfCode;
        public UInt32 BaseOfData;
        public UInt32 ImageBase;
        public UInt32 SectionAlignment;
        public UInt32 FileAlignment;
        public UInt16 MajorOperatingSystemVersion;
        public UInt16 MinorOperatingSystemVersion;
        public UInt16 MajorImageVersion;
        public UInt16 MinorImageVersion;
        public UInt16 MajorSubsystemVersion;
        public UInt16 MinorSubsystemVersion;
        public UInt32 Win32VersionValue;
        public UInt32 SizeOfImage;
        public UInt32 SizeOfHeaders;
        public UInt32 CheckSum;
        public Subsystem Subsystem;
        public UInt16 DllCharacteristics;
        public UInt32 SizeOfStackReserve;
        public UInt32 SizeOfStackCommit;
        public UInt32 SizeOfHeapReserve;
        public UInt32 SizeOfHeapCommit;

        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
        public UInt32 LoaderFlags;

        public UInt32 NumberOfRvaAndSizes;

        public ImageDataDirectory ExportTable;
        public ImageDataDirectory ImportTable;
        public ImageDataDirectory ResourceTable;
        public ImageDataDirectory ExceptionTable;
        public ImageDataDirectory CertificateTable;
        public ImageDataDirectory BaseRelocationTable;
        public ImageDataDirectory Debug;
        public ImageDataDirectory Architecture;
        public ImageDataDirectory GlobalPtr;
        public ImageDataDirectory TLSTable;
        public ImageDataDirectory LoadConfigTable;
        public ImageDataDirectory BoundImport;
        public ImageDataDirectory IAT;
        public ImageDataDirectory DelayImportDescriptor;
        public ImageDataDirectory CLRRuntimeHeader;
        public ImageDataDirectory Reserved;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [CLSCompliant(false)]
    public struct ImageOptionalHeader64
    {
        public UInt16 Magic;
        public Byte MajorLinkerVersion;
        public Byte MinorLinkerVersion;
        public UInt32 SizeOfCode;
        public UInt32 SizeOfInitializedData;
        public UInt32 SizeOfUninitializedData;
        public UInt32 AddressOfEntryPoint;
        public UInt32 BaseOfCode;
        public UInt64 ImageBase;
        public UInt32 SectionAlignment;
        public UInt32 FileAlignment;
        public UInt16 MajorOperatingSystemVersion;
        public UInt16 MinorOperatingSystemVersion;
        public UInt16 MajorImageVersion;
        public UInt16 MinorImageVersion;
        public UInt16 MajorSubsystemVersion;
        public UInt16 MinorSubsystemVersion;
        public UInt32 Win32VersionValue;
        public UInt32 SizeOfImage;
        public UInt32 SizeOfHeaders;
        public UInt32 CheckSum;
        public Subsystem Subsystem;
        public UInt16 DllCharacteristics;
        public UInt64 SizeOfStackReserve;
        public UInt64 SizeOfStackCommit;
        public UInt64 SizeOfHeapReserve;
        public UInt64 SizeOfHeapCommit;

        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
        public UInt32 LoaderFlags;

        public UInt32 NumberOfRvaAndSizes;

        public ImageDataDirectory ExportTable;
        public ImageDataDirectory ImportTable;
        public ImageDataDirectory ResourceTable;
        public ImageDataDirectory ExceptionTable;
        public ImageDataDirectory CertificateTable;
        public ImageDataDirectory BaseRelocationTable;
        public ImageDataDirectory Debug;
        public ImageDataDirectory Architecture;
        public ImageDataDirectory GlobalPtr;
        public ImageDataDirectory TLSTable;
        public ImageDataDirectory LoadConfigTable;
        public ImageDataDirectory BoundImport;
        public ImageDataDirectory IAT;
        public ImageDataDirectory DelayImportDescriptor;
        public ImageDataDirectory CLRRuntimeHeader;
        public ImageDataDirectory Reserved;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [CLSCompliant(false)]
    public struct ImageFileHeader
    {
        public MachineType Machine;
        public UInt16 NumberOfSections;
        public UInt32 TimeDateStamp;
        public UInt32 PointerToSymbolTable;
        public UInt32 NumberOfSymbols;
        public UInt16 SizeOfOptionalHeader;
        public UInt16 Characteristics;
    }
    #endregion

    /// <summary>
    /// Extracts meta data from PE (Portable Executable) file headers.
    /// </summary>
    public class PEHeader
    {
        #region Properties
        public bool Is32BitHeader
        {
            get
            {
                const ushort imageFile32BitMachine = 0x0100;
                return FileHeader.Characteristics.HasFlag(imageFile32BitMachine);
            }
        }

        [CLSCompliant(false)]
        public ImageDosHeader DosHeader { get; private set; }

        [CLSCompliant(false)]
        public ImageFileHeader FileHeader { get; private set; }

        [CLSCompliant(false)]
        public ImageOptionalHeader32 OptionalHeader32 { get; private set; }

        [CLSCompliant(false)]
        public ImageOptionalHeader64 OptionalHeader64 { get; private set; }

        [CLSCompliant(false)]
        public Subsystem Subsystem { get { return Is32BitHeader ? OptionalHeader32.Subsystem : OptionalHeader64.Subsystem; } }
        #endregion

        #region Constructor
        /// <summary>
        /// Reads the PE header of a file.
        /// </summary>
        /// <param name="path">The file to read.</param>
        public PEHeader([NotNull] string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var reader = new BinaryReader(stream);

                DosHeader = Read<ImageDosHeader>(reader);
                stream.Seek(DosHeader.e_lfanew, SeekOrigin.Begin);
                reader.ReadUInt32(); // Skip ntHeadersSignature

                FileHeader = Read<ImageFileHeader>(reader);
                if (Is32BitHeader) OptionalHeader32 = Read<ImageOptionalHeader32>(reader);
                else OptionalHeader64 = Read<ImageOptionalHeader64>(reader);
            }
        }

        private static T Read<T>(BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(Marshal.SizeOf(typeof(T)));
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var structure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return structure;
        }
        #endregion
    }
}
