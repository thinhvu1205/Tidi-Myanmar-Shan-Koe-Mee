// --------------------------------------------------------------------------------------------------------------------
/// <copyright file="HelpAttribute.cs">
///   <See cref="https://github.com/johnearnshaw/unity-inspector-help"></See>
///   Copyright (c) 2017, John Earnshaw, reblGreen Software Limited
///   <See cref="https://github.com/johnearnshaw/"></See>
///   <See cref="https://bitbucket.com/juanshaf/"></See>
///   <See cref="https://reblgreen.com/"></See>
///   All rights reserved.
///   Redistribution and use in source and binary forms, with or without modification, are
///   permitted provided that the following conditions are met:
///      1. Redistributions of source code must retain the above copyright notice, this list of
///         conditions and the following disclaimer.
///      2. Redistributions in binary form must reproduce the above copyright notice, this list
///         of conditions and the following disclaimer in the documentation and/or other materials
///         provided with the distribution.
///   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY
///   EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
///   MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL THE
///   COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
///   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
///   SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
///   HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR
///   TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
///   SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
/// </copyright>
// --------------------------------------------------------------------------------------------------------------------
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GIKCore.Attribute
{
    [AttributeUsage(AttributeTargets.Field, Inherited = true)]
    public class HelpAttribute : PropertyAttribute
    {
        public readonly string text;

        // MessageType exists in UnityEditor namespace and can throw an exception when used outside the editor.
        // We spoof MessageType at the bottom of this script to ensure that errors are not thrown when
        // MessageType is unavailable.
        public readonly MessageType type;


        /// <summary>
        /// Adds a HelpBox to the Unity property inspector above this field.
        /// </summary>
        /// <param name="text">The help text to be displayed in the HelpBox.</param>
        /// <param name="type">The icon to be displayed in the HelpBox.</param>
        public HelpAttribute(string text, Type type = Type.Info)
        {
            this.text = text;
            switch (type)
            {
                case Type.Info:
                    this.type = MessageType.Info;
                    break;
                case Type.Warning:
                    this.type = MessageType.Warning;
                    break;
                case Type.Error:
                    this.type = MessageType.Error;
                    break;
                default:
                    this.type = MessageType.None;
                    break;
            }
        }
    }

    public enum Type
    {
        None,
        Info,
        Warning,
        Error,
    }

#if UNITY_EDITOR
    // do nothing
#else
    // Replicate MessageType Enum if we are not in editor as this enum exists in UnityEditor namespace.
    // This should stop errors being logged the same as Shawn Featherly's commit in the Github repo but I
    // feel is cleaner than having the conditional directive in the middle of the HelpAttribute constructor.
    public enum MessageType
    {
        None,
        Info,
        Warning,
        Error,
    }
#endif
}
