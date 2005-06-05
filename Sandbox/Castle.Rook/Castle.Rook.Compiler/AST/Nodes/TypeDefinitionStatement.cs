// Copyright 2004-2005 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Castle.Rook.Compiler.AST
{
	using System;

	using Castle.Rook.Compiler.Visitors;

	public enum AccessLevel
	{
		Protected,
		Public,
		Private,
		Internal,
	}

	public class TypeDefinitionStatement : AbstractStatement
	{
		private String name;
		private AccessLevel accessLevel;
		private StatementCollection methods;
		private StatementCollection fields;
		private StatementCollection constructors;
		private StatementCollection statements;

		public TypeDefinitionStatement(INameScope parentScope, AccessLevel accessLevel, String name) : base(NodeType.TypeDefinition)
		{
			this.name = name;
			this.accessLevel = accessLevel;
			
			nameScope = new NameScope(NameScopeType.Type, parentScope);

			statements = new StatementCollection(this);
			methods = new StatementCollection(this);
			fields = new StatementCollection(this);
			constructors = new StatementCollection(this);
		}

		public String Name
		{
			get { return name; }
		}

		public AccessLevel AccessLevel
		{
			get { return accessLevel; }
		}

		public StatementCollection Statements
		{
			get { return statements; }
		}

		public StatementCollection Methods
		{
			get { return methods; }
		}

		public StatementCollection Fields
		{
			get { return fields; }
		}

		public StatementCollection Constructors
		{
			get { return constructors; }
		}

		public override bool Accept(IASTVisitor visitor)
		{
			return visitor.VisitTypeDefinitionStatement(this);
		}
	}
}
