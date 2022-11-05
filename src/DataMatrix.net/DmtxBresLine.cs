/*
DataMatrix.Net

DataMatrix.Net - .net library for decoding DataMatrix codes.
Copyright (C) 2009/2010 Michael Faschinger

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public
License as published by the Free Software Foundation; either
version 3.0 of the License, or (at your option) any later version.
You can also redistribute and/or modify it under the terms of the
GNU Lesser General Public License as published by the Free Software
Foundation; either version 3.0 of the License or (at your option)
any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
General Public License or the GNU Lesser General Public License 
for more details.

You should have received a copy of the GNU General Public
License and the GNU Lesser General Public License along with this 
library; if not, write to the Free Software Foundation, Inc., 
51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA

Contact: Michael Faschinger - michfasch@gmx.at
 
*/

using System;

namespace DataMatrix.net
{
    internal struct DmtxBresLine
    {
        internal int XStep { get; set; }
        internal int YStep { get; set; }
        internal int XDelta { get; set; }
        internal int YDelta { get; set; }
        internal bool Steep { get; set; }
        internal int XOut { get; set; }
        internal int YOut { get; set; }
        internal int Travel { get; set; }
        internal int Outward { get; set; }
        internal int Error { get; set; }
        internal DmtxPixelLoc Loc { get; set; }
        internal DmtxPixelLoc Loc0 { get; set; }
        internal DmtxPixelLoc Loc1 { get; set; }

        internal DmtxBresLine(DmtxBresLine orig)
        {
            this.Error = orig.Error;
            this.Loc = new DmtxPixelLoc { X = orig.Loc.X, Y = orig.Loc.Y };
            this.Loc0 = new DmtxPixelLoc { X = orig.Loc0.X, Y = orig.Loc0.Y };
            this.Loc1 = new DmtxPixelLoc { X = orig.Loc1.X, Y = orig.Loc1.Y };
            this.Outward = orig.Outward;
            this.Steep = orig.Steep;
            this.Travel = orig.Travel;
            this.XDelta = orig.XDelta;
            this.XOut = orig.XOut;
            this.XStep = orig.XStep;
            this.YDelta = orig.YDelta;
            this.YOut = orig.YOut;
            this.YStep = orig.YStep;
        }

        internal DmtxBresLine(DmtxPixelLoc loc0, DmtxPixelLoc loc1, DmtxPixelLoc locInside)
        {
            int cp;
            DmtxPixelLoc locBeg, locEnd;


            /* Values that stay the same after initialization */
            this.Loc0 = loc0;
            this.Loc1 = loc1;
            this.XStep = (loc0.X < loc1.X) ? +1 : -1;
            this.YStep = (loc0.Y < loc1.Y) ? +1 : -1;
            this.XDelta = Math.Abs(loc1.X - loc0.X);
            this.YDelta = Math.Abs(loc1.Y - loc0.Y);
            this.Steep = (this.YDelta > this.XDelta);

            /* Take cross product to determine outward step */
            if (this.Steep)
            {
                /* Point first vector up to get correct sign */
                if (loc0.Y < loc1.Y)
                {
                    locBeg = loc0;
                    locEnd = loc1;
                }
                else
                {
                    locBeg = loc1;
                    locEnd = loc0;
                }
                cp = (((locEnd.X - locBeg.X) * (locInside.Y - locEnd.Y)) -
                      ((locEnd.Y - locBeg.Y) * (locInside.X - locEnd.X)));

                this.XOut = (cp > 0) ? +1 : -1;
                this.YOut = 0;
            }
            else
            {
                /* Point first vector left to get correct sign */
                if (loc0.X > loc1.X)
                {
                    locBeg = loc0;
                    locEnd = loc1;
                }
                else
                {
                    locBeg = loc1;
                    locEnd = loc0;
                }
                cp = (((locEnd.X - locBeg.X) * (locInside.Y - locEnd.Y)) -
                      ((locEnd.Y - locBeg.Y) * (locInside.X - locEnd.X)));

                this.XOut = 0;
                this.YOut = (cp > 0) ? +1 : -1;
            }

            /* Values that change while stepping through line */
            this.Loc = loc0;
            this.Travel = 0;
            this.Outward = 0;
            this.Error = (this.Steep) ? this.YDelta / 2 : this.XDelta / 2;
        }

        internal bool GetStep(DmtxPixelLoc target, ref int travel, ref int outward)
        {
            /* Determine necessary step along and outward from Bresenham line */
            if (this.Steep)
            {
                travel = (this.YStep > 0) ? target.Y - this.Loc.Y : this.Loc.Y - target.Y;
                Step(travel, 0);
                outward = (this.XOut > 0) ? target.X - this.Loc.X : this.Loc.X - target.X;
                if (this.YOut != 0)
                {
                    throw new Exception("Invald yOut value for bresline step!");
                }
            }
            else
            {
                travel = (this.XStep > 0) ? target.X - this.Loc.X : this.Loc.X - target.X;
                Step(travel, 0);
                outward = (this.YOut > 0) ? target.Y - this.Loc.Y : this.Loc.Y - target.Y;
                if (this.XOut != 0)
                {
                    throw new Exception("Invald xOut value for bresline step!");
                }
            }

            return true;
        }


        internal bool Step(int travel, int outward)
        {
            int i;

            if (Math.Abs(travel) >= 2)
            {
                throw new ArgumentException("Invalid value for 'travel' in BaseLineStep!");
            }

            /* Perform forward step */
            if (travel > 0)
            {
                this.Travel++;
                if (this.Steep)
                {
                    this.Loc = new DmtxPixelLoc() { X = this.Loc.X, Y = this.Loc.Y + this.YStep };
                    this.Error -= this.XDelta;
                    if (this.Error < 0)
                    {
                        this.Loc = new DmtxPixelLoc() { X = this.Loc.X + this.XStep, Y = this.Loc.Y };
                        this.Error += this.YDelta;
                    }
                }
                else
                {
                    this.Loc = new DmtxPixelLoc() { X = this.Loc.X + this.XStep, Y = this.Loc.Y };
                    this.Error -= this.YDelta;
                    if (this.Error < 0)
                    {
                        this.Loc = new DmtxPixelLoc() { X = this.Loc.X, Y = this.Loc.Y + this.YStep };
                        this.Error += this.XDelta;
                    }
                }
            }
            else if (travel < 0)
            {
                this.Travel--;
                if (this.Steep)
                {
                    this.Loc = new DmtxPixelLoc() { X = this.Loc.X, Y = this.Loc.Y - this.YStep };
                    this.Error += this.XDelta;
                    if (this.Error >= this.YDelta)
                    {
                        this.Loc = new DmtxPixelLoc() { X = this.Loc.X - this.XStep, Y = this.Loc.Y };
                        this.Error -= this.YDelta;
                    }
                }
                else
                {
                    this.Loc = new DmtxPixelLoc() { X = this.Loc.X - this.XStep, Y = this.Loc.Y };
                    this.Error += this.YDelta;
                    if (this.Error >= this.XDelta)
                    {
                        this.Loc = new DmtxPixelLoc() { X = this.Loc.X, Y = this.Loc.Y - this.YStep };
                        this.Error -= this.XDelta;
                    }
                }
            }

            for (i = 0; i < outward; i++)
            {
                /* Outward steps */
                this.Outward++;
                this.Loc = new DmtxPixelLoc() { X = this.Loc.X + this.XOut, Y = this.Loc.Y + this.YOut };
            }

            return true;
        }
    }
}
