﻿/*
    Copyright (C) 2006-2022. Aardvark Platform Team. http://github.com/aardvark-platform.
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using Aardvark.Base;
using Aardvark.Data.Points;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Aardvark.Geometry.Points
{
    /// <summary>
    /// Importers for various formats.
    /// </summary>
    public static partial class PointCloud
    {
        private static IEnumerable<GenericChunk> MergeSmall(int limit, IEnumerable<GenericChunk> input)
        {
            var current = default(GenericChunk);
            foreach (var c in input)
            {
                if (c.Count < limit)
                {
                    if (current != null) current = current.Union(c);
                    else current = c;

                    if (current.Count >= limit)
                    {
                        yield return current;
                        current = null;
                    }

                }
                else
                {
                    yield return c;
                }
            }

            if (current != null)
            {
                yield return current;
            }
        }

        /// <summary>
        /// Imports single chunk.
        /// </summary>
        public static PointSet Chunks(GenericChunk chunk, ImportConfig config)
            => Chunks(new[] { chunk }, config);

        /// <summary>
        /// Imports single chunk.
        /// </summary>
        public static PointSet Import(GenericChunk chunk, ImportConfig config)
            => Chunks(chunk, config);

        /// <summary>
        /// Imports sequence of chunks.
        /// </summary>
        public static PointSet Chunks(IEnumerable<GenericChunk> chunks, ImportConfig config)
        {
            config?.ProgressCallback(0.0);

            if (config.Verbose)
            {
                var chunkCount = 0;
                chunks = chunks.Do(chunk => { Report.Line($"[PointCloud.Chunks] processing chunk {Interlocked.Increment(ref chunkCount)}"); });
            }

            // deduplicate points
            chunks = chunks.Select(x => x.ImmutableDeduplicate(config.Verbose));

            // merge small chunks
            chunks = MergeSmall(config.MaxChunkPointCount, chunks);

            // filter minDist
            if (config.MinDist > 0.0)
            {
                if (config.NormalizePointDensityGlobal)
                {
                    var smallestPossibleCellExponent = Fun.Log2(config.MinDist).Ceiling();
                    chunks = chunks.Select(x =>
                    {
                        var c = new Cell(x.BoundingBox);
                        while (c.Exponent < smallestPossibleCellExponent) c = c.Parent;
                        return x.ImmutableFilterMinDistByCell(c, config.ParseConfig);
                    });
                }
                else
                {
                    chunks = chunks.Select(x => x.ImmutableFilterSequentialMinDistL1(config.MinDist));
                }
            }

            // merge small chunks
            chunks = MergeSmall(config.MaxChunkPointCount, chunks);

            // EXPERIMENTAL
            //Report.BeginTimed("unmix");
            //chunks = chunks.ImmutableUnmixOutOfCore(@"T:\tmp", 1, config);
            //Report.End();

            // reproject positions and/or estimate normals
            if (config.Reproject != null)
            {
                GenericChunk map(GenericChunk x, CancellationToken ct)
                {
                    if (config.Reproject != null)
                    {
                        x = x.Positions switch
                        {
                            V2f[] ps => x.WithPositions(config.Reproject(ps.Map(p => (V3d)p.XYO)).Map(p => (V2f)p.XY)),
                            V2d[] ps => x.WithPositions(config.Reproject(ps.Map(p => p.XYO)).Map(p => p.XY)),
                            V3f[] ps => x.WithPositions(config.Reproject(ps.Map(p => (V3d)p)).Map(p => (V3f)p)),
                            V3d[] ps => x.WithPositions(config.Reproject(ps)),
                            _ => throw new Exception($"Unsupported positions type {x.Positions.GetType()}."),
                        };
                    }

                    return x;
                }

                chunks = chunks.MapParallel(map, config.MaxDegreeOfParallelism, null, config.CancellationToken);
            }

            // reduce all chunks to single PointSet
            if (config.Verbose) Report.BeginTimed("map/reduce");
            var final = chunks
                .MapReduce(config.WithRandomKey().WithProgressCallback(x => config.ProgressCallback(0.01 + x * 0.65)))
                ;
            if (config.Verbose) Report.EndTimed();

            // create LOD data
            if (config.Verbose) Report.BeginTimed("generate lod");
            final = final.GenerateLod(config.WithRandomKey().WithProgressCallback(x => config.ProgressCallback(0.66 + x * 0.34)));
            if (final.Root != null && config.Storage.GetPointCloudNode(final.Root.Value.Id) == null) throw new InvalidOperationException("Invariant 4d633e55-bf84-45d7-b9c3-c534a799242e.");
            if (config.Verbose) Report.End();

            // create final point set with specified key (or random key when no key is specified)
            var key = config.Key ?? Guid.NewGuid().ToString();
            final = new PointSet(config.Storage, key, final?.Root?.Value?.Id, config.OctreeSplitLimit);
            config.Storage.Add(key, final);

            return final;
        }

        /// <summary>
        /// Imports sequence of chunks.
        /// </summary>
        public static PointSet Import(IEnumerable<GenericChunk> chunks, ImportConfig config) => Chunks(chunks, config);
    }
}
