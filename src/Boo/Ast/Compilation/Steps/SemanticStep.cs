using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using Boo;
using Boo.Ast;
using Boo.Ast.Compilation;
using Boo.Ast.Compilation.NameBinding;

namespace Boo.Ast.Compilation.Steps
{		
	/// <summary>
	/// Step 4.
	/// </summary>
	public class SemanticStep : AbstractCompilerStep
	{
		ModuleBuilder _moduleBuilder;
		
		TypeBuilder _typeBuilder;
		
		Method _method;
		
		INameSpace _namespace;
		
		public override void Run()
		{
			_moduleBuilder = AssemblySetupStep.GetModuleBuilder(CompilerContext);
			
			Switch(CompileUnit);
		}
		
		public override bool EnterCompileUnit(CompileUnit cu)
		{			
			// builtins resolution at the highest level
			_namespace = new TypeNameSpace(BindingManager, null, typeof(Boo.Lang.Builtins));
			return true;
		}
		
		public override bool EnterModule(Boo.Ast.Module module)
		{
			TypeAttributes attributes = TypeAttributes.Public|TypeAttributes.Sealed;
			_typeBuilder = _moduleBuilder.DefineType(module.FullyQualifiedName, attributes);
			
			BindingManager.Bind(module, _typeBuilder);
			
			_namespace = new TypeDefinitionNameSpace(BindingManager, _namespace, module);
			return true;
		}
		
		public override void OnMethod(Method method)
		{
			_method = method;
			
			ProcessParameters(method);
			ProcessReturnType(method);
			
			_namespace = new MethodNameSpace(BindingManager, _namespace, _method);			
			
			MethodBuilder mbuilder = _typeBuilder.DefineMethod(method.Name,
				                     MethodAttributes.Static|MethodAttributes.Private,
				                     BindingManager.GetBoundType(method.ReturnType),
				                     GetParameterTypes(method));
			BindingManager.Bind(method, mbuilder);
			
			method.Body.Switch(this);
		}
		
		public override void OnTypeReference(TypeReference node)
		{
			INameBinding info = ResolveName(node, node.Name);
			if (null != info)
			{
				if (NameBindingType.Type != info.BindingType)
				{
					Errors.NameNotType(node, node.Name);
				}
				else
				{
					BindingManager.Bind(node, info);
				}
			}
		}
		
		void ProcessParameters(Method method)
		{
			ParameterDeclarationCollection parameters = method.Parameters;
			for (int i=0; i<parameters.Count; ++i)
			{
				ParameterDeclaration parameter = parameters[i];
				if (null == parameter.Type)
				{
					parameter.Type = new TypeReference("object");
					BindingManager.Bind(parameter.Type, BindingManager.ToTypeBinding(BindingManager.ObjectType));
				}		
				else
				{
					parameter.Type.Switch(this);
				}
				NameBinding.ParameterBinding info = new NameBinding.ParameterBinding(parameter, BindingManager.GetTypeBinding(parameter.Type), i);
				BindingManager.Bind(parameter, info);
			}
		}
		
		void ProcessReturnType(Method method)
		{
			if (null == method.ReturnType)
			{
				// Por enquanto, valor de retorno apenas void
				method.ReturnType = new TypeReference("void");
				BindingManager.Bind(method.ReturnType, BindingManager.ToTypeBinding(BindingManager.VoidType));
			}
			else
			{
				if (!BindingManager.IsBound(method.ReturnType))
				{
					method.ReturnType.Switch(this);
				}
			}
		}
		
		public override void OnStringLiteralExpression(StringLiteralExpression node)
		{
			BindingManager.Bind(node, BindingManager.StringType);
		}
		
		public override void OnReferenceExpression(ReferenceExpression node)
		{
			INameBinding info = ResolveName(node, node.Name);
			if (null != info)
			{
				BindingManager.Bind(node, info);
			}
		}
		
		public override void OnBinaryExpression(BinaryExpression node)
		{
			if (node.Operator == BinaryOperatorType.Assign)
			{
				// Auto local declaration:
				// assign to unknown reference implies local
				// declaration
				ReferenceExpression reference = node.Left as ReferenceExpression;
				if (null != reference)
				{
					node.Right.Switch(this);
					
					ITypeBinding expressionTypeInfo = BindingManager.GetTypeBinding(node.Right);
					
					INameBinding info = _namespace.Resolve(reference.Name);					
					if (null == info)
					{
						Local local = new Local(reference);
						LocalBinding localInfo = new LocalBinding(local, expressionTypeInfo);
						BindingManager.Bind(local, localInfo);
						
						_method.Locals.Add(local);
						BindingManager.Bind(reference, localInfo);
					}
					else
					{
						// default reference resolution
						if (CheckNameResolution(reference, reference.Name, info))
						{
							BindingManager.Bind(reference, info);
						}
					}
					
					LeaveBinaryExpression(node);
				}
				else
				{
					throw new NotImplementedException();
				}
			}
			else
			{
				base.OnBinaryExpression(node);
			}
		}
		
		public override void LeaveBinaryExpression(BinaryExpression node)
		{
			// expression type is the same as the right expression's
			BindingManager.Bind(node, BindingManager.GetBinding(node.Right));
		}
		
		public override void LeaveMethodInvocationExpression(MethodInvocationExpression node)
		{			
			INameBinding targetType = BindingManager.GetBinding(node.Target);			
			if (targetType.BindingType == NameBindingType.Method)
			{				
				IMethodBinding targetMethod = (IMethodBinding)targetType;
				CheckParameters(targetMethod, node);
				
				// 1) conferir número de parâmetros ao método
				// 2) conferir compatibilidade dos parâmetros				
				BindingManager.Bind(node, targetMethod.ReturnType);
			}
			else
			{
				throw new NotImplementedException();
			}
		}
		
		INameBinding ResolveName(Node node, string name)
		{
			INameBinding info = null;
			switch (name)
			{
				case "void":
				{
					info = BindingManager.ToTypeBinding(BindingManager.VoidType);
					break;
				}
				
				case "string":
				{
					info = BindingManager.ToTypeBinding(BindingManager.StringType);
					break;
				}
				
				default:
				{
					info = _namespace.Resolve(name);
					CheckNameResolution(node, name, info);
					break;
				}
			}			
			return info;
		}
		
		bool CheckNameResolution(Node node, string name, INameBinding info)
		{
			if (null == info)
			{
				Errors.UnknownName(node, name);			
				return false;
			}
			else
			{
				if (info.BindingType == NameBindingType.AmbiguousName)
				{
					//Errors.AmbiguousName(node, name, info);
					//return false;
					throw new NotImplementedException();
				}
			}
			return true;
		}
		
		void CheckParameters(IMethodBinding method, MethodInvocationExpression mie)
		{			
			if (method.ParameterCount != mie.Arguments.Count)
			{
				Errors.MethodArgumentCount(mie, method);
			}
		}
		
		Type[] GetParameterTypes(Method method)
		{
			ParameterDeclarationCollection parameters = method.Parameters;
			Type[] types = new Type[parameters.Count];
			for (int i=0; i<types.Length; ++i)
			{
				types[i] = BindingManager.GetBoundType(parameters[i].Type);
			}
			return types;
		}
	}
}
