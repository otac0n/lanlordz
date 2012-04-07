//-----------------------------------------------------------------------
// <copyright file="Exceptions.cs" company="LAN Lordz, inc.">
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

namespace LanLordz
{
    using System;

    [Serializable]
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException() { }

        public UserNotFoundException(string message) : base(message) { }

        public UserNotFoundException(string message, Exception inner) : base(message, inner) { }

        protected UserNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class UserAlreadyRegisteredException : Exception
    {
        public UserAlreadyRegisteredException() { }

        public UserAlreadyRegisteredException(string message) : base(message) { }

        public UserAlreadyRegisteredException(string message, Exception inner) : base(message, inner) { }

        protected UserAlreadyRegisteredException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class EmailNotConfirmedException : Exception
    {
        public EmailNotConfirmedException() { }

        public EmailNotConfirmedException(string message) : base(message) { }

        public EmailNotConfirmedException(string message, Exception inner) : base(message, inner) { }

        protected EmailNotConfirmedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class HostLockedOutException : Exception
    {
        public HostLockedOutException() { }

        public HostLockedOutException(string message) : base(message) { }

        public HostLockedOutException(string message, Exception inner) : base(message, inner) { }

        protected HostLockedOutException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public class UserLockedOutException : Exception
    {
        public UserLockedOutException() { }

        public UserLockedOutException(string message) : base(message) { }

        public UserLockedOutException(string message, Exception inner) : base(message, inner) { }

        protected UserLockedOutException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}