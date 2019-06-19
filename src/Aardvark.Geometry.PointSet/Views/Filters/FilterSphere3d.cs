﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Aardvark.Base;
using Newtonsoft.Json.Linq;

namespace Aardvark.Geometry.Points
{
    /// <summary></summary>
    public class FilterInsideSphere3d : ISpatialFilter
    {
        private readonly Sphere3d m_sphere;
        private readonly double m_radiusSquared;

        /// <summary></summary>
        public const string Type = "FilterInsideSphere3d";

        private bool Contains(V3d pt)
        {
            return V3d.DistanceSquared(m_sphere.Center, pt) <= m_radiusSquared;
        }

        /// <summary></summary>
        public FilterInsideSphere3d(Sphere3d sphere)
        {
            m_sphere = sphere;
            m_radiusSquared = sphere.RadiusSquared;
        }

        /// <summary></summary>
        public HashSet<int> FilterPoints(IPointCloudNode node, HashSet<int> selected = null)
        {
            if (selected != null)
            {
                var c = node.Center;
                var ps = node.Positions.Value;
                return new HashSet<int>(selected.Where(i => Contains(c + (V3d)ps[i])));
            }
            else
            {
                var c = node.Center;
                var ps = node.Positions.Value;
                var result = new HashSet<int>();
                for (var i = 0; i < ps.Length; i++)
                {
                    if (Contains(c + (V3d)ps[i])) result.Add(i);
                }
                return result;
            }
        }

        /// <summary></summary>
        public bool IsFullyInside(Box3d box)
        {
            return box.ComputeCorners().TrueForAll(Contains);
        }
        /// <summary></summary>
        public bool IsFullyInside(IPointCloudNode node)
        {
            return IsFullyInside(node.BoundingBoxExactGlobal);
        }

        /// <summary></summary>
        public bool IsFullyOutside(Box3d box)
        {
            return !box.Intersects(m_sphere);
        }

        /// <summary></summary>
        public bool IsFullyOutside(IPointCloudNode node)
        {
            return IsFullyOutside(node.BoundingBoxExactGlobal);
        }

        /// <summary></summary>
        public JObject Serialize()
        {
            return JObject.FromObject(new { Type, Sphere = m_sphere.ToString() });
        }
    }
}
