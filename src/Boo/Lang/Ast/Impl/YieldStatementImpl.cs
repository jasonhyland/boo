﻿#region license
// boo - an extensible programming language for the CLI
// Copyright (C) 2004 Rodrigo B. de Oliveira
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// As a special exception, if you link this library with other files to
// produce an executable, this library does not by itself cause the
// resulting executable to be covered by the GNU General Public License.
// This exception does not however invalidate any other reasons why the
// executable file might be covered by the GNU General Public License.
//
// Contact Information
//
// mailto:rbo@acm.org
#endregion

//
// DO NOT EDIT THIS FILE!
//
// This file was generated automatically by
// astgenerator.boo on 3/26/2004 11:12:00 AM
//

namespace Boo.Lang.Ast.Impl
{	
	using Boo.Lang.Ast;
	using System.Collections;
	using System.Runtime.Serialization;
	
	[System.Serializable]
	public abstract class YieldStatementImpl : Statement
	{

		protected Expression _expression;

		protected YieldStatementImpl()
		{
			InitializeFields();
		}
		
		protected YieldStatementImpl(LexicalInfo info) : base(info)
		{
			InitializeFields();
		}
		

		protected YieldStatementImpl(Expression expression)
		{
			InitializeFields();
			Expression = expression;
		}
			
		protected YieldStatementImpl(LexicalInfo lexicalInfo, Expression expression) : base(lexicalInfo)
		{
			InitializeFields();
			Expression = expression;
		}
			
		new public YieldStatement CloneNode()
		{
			return Clone() as YieldStatement;
		}

		override public NodeType NodeType
		{
			get
			{
				return NodeType.YieldStatement;
			}
		}
		
		override public void Switch(IAstTransformer transformer, out Node resultingNode)
		{
			YieldStatement thisNode = (YieldStatement)this;
			Statement resultingTypedNode = thisNode;
			transformer.OnYieldStatement(thisNode, ref resultingTypedNode);
			resultingNode = resultingTypedNode;
		}

		override public bool Replace(Node existing, Node newNode)
		{
			if (base.Replace(existing, newNode))
			{
				return true;
			}

			if (_modifier == existing)
			{
				this.Modifier = (StatementModifier)newNode;
				return true;
			}

			if (_expression == existing)
			{
				this.Expression = (Expression)newNode;
				return true;
			}

			return false;
		}

		override public object Clone()
		{
			YieldStatement clone = FormatterServices.GetUninitializedObject(typeof(YieldStatement)) as YieldStatement;
			clone._lexicalInfo = _lexicalInfo;
			clone._documentation = _documentation;
			clone._properties = _properties.Clone() as Hashtable;
			

			if (null != _modifier)
			{
				clone._modifier = _modifier.Clone() as StatementModifier;
				clone._modifier.InitializeParent(clone);
			}

			if (null != _expression)
			{
				clone._expression = _expression.Clone() as Expression;
				clone._expression.InitializeParent(clone);
			}
			
			return clone;
		}
			
		public Expression Expression
		{
			get
			{
				return _expression;
			}
			

			set
			{
				if (_expression != value)
				{
					_expression = value;
					if (null != _expression)
					{
						_expression.InitializeParent(this);

					}
				}
			}
			

		}
		

		private void InitializeFields()
		{

		}
	}
}
