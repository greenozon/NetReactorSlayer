﻿/*
    Copyright (C) 2011-2015 de4dot@gmail.com

    This file is part of de4dot.

    de4dot is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    de4dot is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with de4dot.  If not, see <http://www.gnu.org/licenses/>.
*/

using de4dot.blocks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;
using dnlib.PE;
using System.Collections.Generic;

namespace NetReactorSlayer.Core.Utils.de4dot
{
    public class DumpedMethodsRestorer : IRowReader<RawMethodRow>, IColumnReader, IMethodDecrypter
    {
        ModuleDefMD module;
        readonly DumpedMethods dumpedMethods;

        public ModuleDefMD Module
        {
            set => module = value;
        }

        public DumpedMethodsRestorer(DumpedMethods dumpedMethods) => this.dumpedMethods = dumpedMethods;

        DumpedMethod GetDumpedMethod(uint rid) => dumpedMethods.Get(0x06000000 | rid);

        public bool TryReadRow(uint rid, out RawMethodRow row)
        {
            var dm = GetDumpedMethod(rid);
            if (dm == null)
            {
                row = default;
                return false;
            }
            else
            {
                row = new RawMethodRow(dm.mdRVA, dm.mdImplFlags, dm.mdFlags, dm.mdName, dm.mdSignature, dm.mdParamList);
                return true;
            }
        }

        public bool ReadColumn(MDTable table, uint rid, ColumnInfo column, out uint value)
        {
            if (table.Table == Table.Method)
            {
                if (TryReadRow(rid, out var row))
                {
                    value = row[column.Index];
                    return true;
                }
            }

            value = 0;
            return false;
        }

        public bool GetMethodBody(uint rid, RVA rva, IList<Parameter> parameters, GenericParamContext gpContext, out MethodBody methodBody)
        {
            var dm = GetDumpedMethod(rid);
            if (dm == null)
            {
                methodBody = null;
                return false;
            }
            methodBody = MethodBodyReader.CreateCilBody(module, dm.code, dm.extraSections, parameters, dm.mhFlags, dm.mhMaxStack, dm.mhCodeSize, dm.mhLocalVarSigTok, gpContext);
            return true;
        }
    }
}