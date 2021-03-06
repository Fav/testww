//========================= (UNCLASSIFIED) ==============================
// Copyright � 2005-2006 The Johns Hopkins University /
// Applied Physics Laboratory.  All rights reserved.
//
// WorldWind Source Code - Copyright 2005 NASA World Wind 
// Modified under the NOSA License
//
//========================= (UNCLASSIFIED) ==============================
//
// LICENSE AND DISCLAIMER 
//
// Copyright (c) 2005 The Johns Hopkins University. 
//
// This software was developed at The Johns Hopkins University/Applied 
// Physics Laboratory (�JHU/APL�) that is the author thereof under the 
// �work made for hire� provisions of the copyright law.  Permission is 
// hereby granted, free of charge, to any person obtaining a copy of this 
// software and associated documentation (the �Software�), to use the 
// Software without restriction, including without limitation the rights 
// to copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit others to do so, subject to the 
// following conditions: 
//
// 1.  This LICENSE AND DISCLAIMER, including the copyright notice, shall 
//     be included in all copies of the Software, including copies of 
//     substantial portions of the Software; 
//
// 2.  JHU/APL assumes no obligation to provide support of any kind with 
//     regard to the Software.  This includes no obligation to provide 
//     assistance in using the Software nor to provide updated versions of 
//     the Software; and 
//
// 3.  THE SOFTWARE AND ITS DOCUMENTATION ARE PROVIDED AS IS AND WITHOUT 
//     ANY EXPRESS OR IMPLIED WARRANTIES WHATSOEVER.  ALL WARRANTIES 
//     INCLUDING, BUT NOT LIMITED TO, PERFORMANCE, MERCHANTABILITY, FITNESS
//     FOR A PARTICULAR PURPOSE, AND NONINFRINGEMENT ARE HEREBY DISCLAIMED.  
//     USERS ASSUME THE ENTIRE RISK AND LIABILITY OF USING THE SOFTWARE.  
//     USERS ARE ADVISED TO TEST THE SOFTWARE THOROUGHLY BEFORE RELYING ON 
//     IT.  IN NO EVENT SHALL THE JOHNS HOPKINS UNIVERSITY BE LIABLE FOR 
//     ANY DAMAGES WHATSOEVER, INCLUDING, WITHOUT LIMITATION, ANY LOST 
//     PROFITS, LOST SAVINGS OR OTHER INCIDENTAL OR CONSEQUENTIAL DAMAGES, 
//     ARISING OUT OF THE USE OR INABILITY TO USE THE SOFTWARE. 
//
using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using System.IO;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

using WorldWind;
using WorldWind.Renderable;
using System.Runtime.InteropServices;

namespace WorldWind.NewWidgets
{
    public class UpdateWidget : TreeNodeWidget
    {
        public const int NODE_UPDATE_SIZE = 15;

        protected static Microsoft.DirectX.Direct3D.Font m_textFont;

        protected MouseClickAction m_updateClickAction;
        public MouseClickAction UpdateClickAction
        {
            get { return m_updateClickAction; }
            set { m_updateClickAction = value; }
        }

        public new bool OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            if (m_visible)
            {
                // if we're in the client area
                if (e.X >= this.AbsoluteLocation.X &&
                    e.X <= this.AbsoluteLocation.X + ClientSize.Width &&
                    e.Y >= this.AbsoluteLocation.Y &&
                    e.Y <= this.AbsoluteLocation.Y + NODE_HEIGHT)
                {
                    if (m_isMouseDown)
                    {
                        // if we're in the update region
                        if ((e.X > this.AbsoluteLocation.X + m_xOffset + NODE_ARROW_SIZE + NODE_CHECKBOX_SIZE) &&
                            (e.X < this.AbsoluteLocation.X + m_xOffset + NODE_ARROW_SIZE + NODE_CHECKBOX_SIZE + NODE_UPDATE_SIZE))
                        {
                            if (m_updateClickAction != null)
                                m_updateClickAction(e);
                        }
                    }
                }
            }
            return ((TreeNodeWidget)(this)).OnMouseUp(e);
        }
        public override int Render(DrawArgs drawArgs, int xOffset, int yOffset)
        {
            m_ConsumedSize.Height = 0;
            if (m_textFont == null)
            {
                System.Drawing.Font localHeaderFont = new System.Drawing.Font("Arial", 12.0f, FontStyle.Bold);
                m_textFont = new Microsoft.DirectX.Direct3D.Font(drawArgs.device, localHeaderFont);
            }
            if (m_visible)
            {
                if (!m_isInitialized)
                    this.Initialize(drawArgs);

                m_ConsumedSize.Height = NODE_HEIGHT;

                // This value is dynamic based on the number of expanded nodes above this one
                m_location.Y = yOffset;

                // store this value so the mouse events can figure out where the buttons are
                m_xOffset = xOffset;

                // compute the color
                int color = this.Enabled ? m_itemOnColor : m_itemOffColor;

                // create the bounds of the text draw area
                Rectangle bounds = new Rectangle(this.AbsoluteLocation, new System.Drawing.Size(this.ClientSize.Width, NODE_HEIGHT));

                if (m_isMouseOver)
                {
                    if (!Enabled)
                        color = m_mouseOverOffColor;

                    WidgetUtilities.DrawBox(
                        bounds.X,
                        bounds.Y,
                        bounds.Width,
                        bounds.Height,
                        0.0f,
                        m_mouseOverColor,
                        drawArgs.device);
                }

                #region Draw arrow

                bounds.X = this.AbsoluteLocation.X + xOffset;
                bounds.Width = NODE_ARROW_SIZE;
                // draw arrow if any children
                if (m_subNodes.Count > 0)
                {
                    m_worldwinddingsFont.DrawText(
                        null,
                        (this.m_isExpanded ? "L" : "A"),
                        bounds,
                        DrawTextFormat.None,
                        color);
                }
                #endregion Draw arrow

                #region Draw checkbox

                bounds.Width = NODE_CHECKBOX_SIZE;
                bounds.X += NODE_ARROW_SIZE;

                // Normal check symbol
                string checkSymbol;

                if (m_isRadioButton)
                {
                    checkSymbol = this.IsChecked ? "O" : "P";
                }
                else
                {
                    checkSymbol = this.IsChecked ? "N" : "F";
                }

                m_worldwinddingsFont.DrawText(
                    null,
                    checkSymbol,
                    bounds,
                    DrawTextFormat.NoClip,
                    color);

                #endregion draw checkbox

                #region Draw update

                bounds.X += NODE_CHECKBOX_SIZE;
                bounds.Width = NODE_UPDATE_SIZE;

                string updateSymbol = "U";

                //m_worldwinddingsFont
                m_textFont.DrawText(null, updateSymbol, bounds, DrawTextFormat.NoClip, color);

                #endregion draw update


                #region Draw name

                // compute the length based on name length 
                // TODO: Do this only when the name changes
                Rectangle stringBounds = drawArgs.defaultDrawingFont.MeasureString(null, Name, DrawTextFormat.NoClip, 0);
                m_size.Width = NODE_ARROW_SIZE + NODE_CHECKBOX_SIZE +NODE_UPDATE_SIZE+ 5 + stringBounds.Width;
                m_ConsumedSize.Width = m_size.Width;

                bounds.Y += 2;
                bounds.X += NODE_UPDATE_SIZE+ 5;
                bounds.Width = stringBounds.Width;

                drawArgs.defaultDrawingFont.DrawText(
                    null,
                    Name,
                    bounds,
                    DrawTextFormat.None,
                    color);

                #endregion Draw name

                if (m_isExpanded)
                {
                    int newXOffset = xOffset + NODE_INDENT;

                    for (int i = 0; i < m_subNodes.Count; i++)
                    {
                        if (m_subNodes[i] is TreeNodeWidget)
                        {
                            m_ConsumedSize.Height += ((TreeNodeWidget)m_subNodes[i]).Render(drawArgs, newXOffset, m_ConsumedSize.Height);
                        }
                        else
                        {
                            System.Drawing.Point newLocation = m_subNodes[i].Location;
                            newLocation.Y = m_ConsumedSize.Height;
                            newLocation.X = newXOffset;
                            m_ConsumedSize.Height += m_subNodes[i].WidgetSize.Height;
                            m_subNodes[i].Location = newLocation;
                            m_subNodes[i].Render(drawArgs);
                            // render normal widgets as a stack of widgets
                        }

                        // if the child width is bigger than my width save it as the consumed width for widget size calculations
                        if (m_subNodes[i].WidgetSize.Width + newXOffset > m_ConsumedSize.Width)
                        {
                            m_ConsumedSize.Width = m_subNodes[i].WidgetSize.Width + newXOffset;
                        }
                    }
                }
            }
            return m_ConsumedSize.Height;
        }
    }
}