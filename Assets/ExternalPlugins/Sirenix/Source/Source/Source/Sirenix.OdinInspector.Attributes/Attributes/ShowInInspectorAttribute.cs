//-----------------------------------------------------------------------
// <copyright file="ShowInInspectorAttribute.cs" company="Sirenix IVS">
// Copyright (c) Sirenix IVS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Sirenix.OdinInspector
{
    using System;

    /// <summary>
	/// <para>ShowInInspector is used on any property, and shows the value in the inspector.</para>
    /// </summary>
	/// <remarks>
    /// <para>This can for example be combined with <see cref="ReadOnlyAttribute"/> to allow for live debugging of values.</para>
    /// <note type="note"></note>
    /// </remarks>
	/// <example>
	/// <para>The following example shows how ShowInInspector is used to show properties in the inspector, that otherwise wouldn't.</para>
    /// <code>
	///	public class MyComponent : MonoBehaviour
	///	{
	///		[ShowInInspector]
	///		private int myField;
	///
	///		[ShowInInspector]
	///		public int MyProperty { get; set; }
	///	}
	/// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ShowInInspectorAttribute : Attribute
    {
    }
}