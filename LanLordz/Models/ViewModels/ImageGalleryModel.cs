//-----------------------------------------------------------------------
// <copyright file=".cs" company="LAN Lordz, inc.">
//  Copyright © 2010 LAN Lordz, inc.
//
//  This file is part of The LAN Lordz LAN Party System.
//
//  The LAN Lordz LAN Party System is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  The LAN Lordz LAN Party System is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with The LAN Lordz LAN Party System.  If not, see [http://www.gnu.org/licenses/].
// </copyright>
// <author>John Gietzen</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using LanLordz.Models.CachedModels;

namespace LanLordz.Models
{
    public class ImageGalleryModel
    {
        public UserInformation User { get; set; }

        public CachedEvent Event { get; set; }

        public IEnumerable<EventImage> Images { get; set; }

        public PaginationInformation PageInfo { get; set; }
    }
}
