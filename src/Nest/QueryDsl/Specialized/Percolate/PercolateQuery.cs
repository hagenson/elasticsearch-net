﻿using System;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Nest
{
	/// <summary>
	/// The percolate query can be used to match queries stored in an index
	/// </summary>
	[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
	[JsonConverter(typeof(ReadAsTypeJsonConverter<PercolateQuery>))]
	public interface IPercolateQuery : IQuery
	{
		/// <summary>
		/// The name of the field containing the percolated query on an existing document. This is a required parameter.
		/// </summary>
		[JsonProperty("field")]
		Field Field { get; set; }

		/// <summary>
		/// The type / mapping of the document to percolate. This is a required parameter.
		/// </summary>
		[JsonProperty("document_type")]
		[Obsolete("Deprecated in 6.x, types are gone from indices created as of Elasticsearch 6.x")]
		TypeName DocumentType { get; set; }

		/// <summary>
		/// The source of the document to percolate.
		/// </summary>
		[JsonProperty("document")]
		[JsonConverter(typeof(SourceConverter))]
		object Document { get; set; }

		/// <summary>
		/// Like the document parameter, but accepts multiple documents.
		/// </summary>
		[JsonProperty("documents")]
		[JsonConverter(typeof(SourceConverter))]
		IEnumerable<object> Documents { get; set; }

		/// <summary>
		/// The id of the document to fetch for percolation.
		/// Can be specified to percolate an existing document instead of providing <see cref="Document"/>
		/// </summary>
		[JsonProperty("id")]
		Id Id { get; set; }

		/// <summary>
		/// The index the document resides in for percolation.
		/// Can be specified to percolate an existing document instead of providing <see cref="Document"/>
		/// </summary>
		[JsonProperty("index")]
		IndexName Index { get; set; }

		/// <summary>
		/// The type of the document to fetch for percolation.
		/// Can be specified to percolate an existing document instead of providing <see cref="Document"/>
		/// </summary>
		[JsonProperty("type")]
		TypeName Type { get; set; }

		/// <summary>
		/// Routing to be used to fetch the document to percolate.
		/// Can be specified to percolate an existing document instead of providing <see cref="Document"/>
		/// </summary>
		[JsonProperty("routing")]
		Routing Routing { get; set; }

		/// <summary>
		/// Preference to be used to fetch the document to percolate.
		/// Can be specified to percolate an existing document instead of providing <see cref="Document"/>
		/// </summary>
		[JsonProperty("preference")]
		string Preference { get; set; }

		/// <summary>
		/// The expected version of the document to be fetched for percolation.
		/// Can be specified to percolate an existing document instead of providing <see cref="Document"/>
		/// </summary>
		[JsonProperty("version")]
		long? Version { get; set; }
	}

	/// <summary>
	/// The percolate query can be used to match queries stored in an index
	/// </summary>
	public class PercolateQuery : QueryBase, IPercolateQuery
	{
		protected override bool Conditionless => IsConditionless(this);

		internal override void InternalWrapInContainer(IQueryContainer c) => c.Percolate = this;

		internal static bool IsConditionless(IPercolateQuery q)
		{
			var docFields = q.Document == null && (q.Documents == null || q.Documents.Count() == 0);
			if (!docFields) return false;

			return q.Type.IsConditionless() ||
			       q.Index == null ||
			       q.Id.IsConditionless() ||
			       q.Field.IsConditionless();
		}

		/// <summary>
		/// The name fo the field containing the percolated query on an existing document. This is a required parameter.
		/// </summary>
		public Field Field { get; set; }

		/// <summary>
		/// The type / mapping of the document to percolate. This is a required parameter.
		/// </summary>
		[Obsolete("Deprecated in 6.x, types are gone from indices created as of Elasticsearch 6.x")]
		public TypeName DocumentType { get; set; }

		/// <summary>
		/// The source of the document to percolate.
		/// </summary>
		public object Document { get; set; }

		/// <summary>
		/// Like the document parameter, but accepts multiple documents.
		/// </summary>
		public IEnumerable<object> Documents { get; set; }

		/// <summary>
		/// The id of the document to fetch for percolation.
		/// Can be specified to percolate an existing document instead of providing <see cref="Document"/>
		/// </summary>
		public Id Id { get; set; }

		/// <summary>
		/// The index the document resides in for percolation.
		/// Can be specified to percolate an existing document instead of providing <see cref="Document"/>
		/// </summary>
		public IndexName Index { get; set; }

		/// <summary>
		/// The type of the document to fetch for percolation.
		/// Can be specified to percolate an existing document instead of providing <see cref="Document"/>
		/// </summary>
		public TypeName Type { get; set; }

		private Routing _routing;
		/// <summary>
		/// Routing to be used to fetch the document to percolate.
		/// Can be specified to percolate an existing document instead of providing <see cref="Document"/>
		/// </summary>
		public Routing Routing
		{
			get => _routing ?? (Document == null && (Document == null || Documents.Count() == 0)? null : new Routing(Document ?? Documents.First()));
			set => _routing = value;
		}

		/// <summary>
		/// Preference to be used to fetch the document to percolate.
		/// Can be specified to percolate an existing document instead of providing <see cref="Document"/>
		/// </summary>
		public string Preference { get; set; }

		/// <summary>
		/// The expected version of the document to be fetched for percolation.
		/// Can be specified to percolate an existing document instead of providing <see cref="Document"/>
		/// </summary>
		public long? Version { get; set; }
	}

	/// <summary>
	/// The percolate query can be used to match queries stored in an index
	/// </summary>
	/// <typeparam name="T">The document type that contains the percolated query</typeparam>
	public class PercolateQueryDescriptor<T>
		: QueryDescriptorBase<PercolateQueryDescriptor<T>, IPercolateQuery>
		, IPercolateQuery where T : class
	{
		Field IPercolateQuery.Field { get; set; }
		TypeName IPercolateQuery.DocumentType { get; set; }
		object IPercolateQuery.Document { get; set; }
		IEnumerable<object> IPercolateQuery.Documents { get; set; }
		Id IPercolateQuery.Id { get; set; }
		IndexName IPercolateQuery.Index { get; set; }
		TypeName IPercolateQuery.Type { get; set; }

		private Routing _routing;
		Routing IPercolateQuery.Routing
		{
			get => _routing ?? (Self.Document == null && (Self.Documents == null || Self.Documents.Count() == 0)? null : new Routing(Self.Document ?? Self.Documents.First()));
			set => _routing = value;
		}

		string IPercolateQuery.Preference { get; set; }
		long? IPercolateQuery.Version { get; set; }

		/// <summary>
		/// Determines if the query is conditionless and should not be serialized
		/// in the request
		/// </summary>
		protected override bool Conditionless => PercolateQuery.IsConditionless(this);

		/// <summary>
		/// An expression for the name fo the field containing the percolated query on an existing document. This is a required parameter.
		/// </summary>
		public PercolateQueryDescriptor<T> Field(Field field) => Assign(a => a.Field = field);

		/// <summary>
		/// The name fo the field containing the percolated query on an existing document. This is a required parameter.
		/// </summary>
		public PercolateQueryDescriptor<T> Field(Expression<Func<T, object>> objectPath) => Assign(a => a.Field = objectPath);

		/// <summary>
		/// The type / mapping of the document to percolate. This is a required parameter.
		/// </summary>
		[Obsolete("Deprecated in 6.x, types are gone from indices created as of Elasticsearch 6.x")]
		public PercolateQueryDescriptor<T> DocumentType(TypeName type) => Assign(a => a.DocumentType = type);

		/// <summary>
		/// The type / mapping of the document to percolate. This is a required parameter.
		/// </summary>
		[Obsolete("Deprecated in 6.x, types are gone from indices created as of Elasticsearch 6.x")]
		public PercolateQueryDescriptor<T> DocumentType<TDocument>() => Assign(a => a.DocumentType = typeof(TDocument));

		/// <summary>
		/// The source of the document to percolate.
		/// </summary>
		public PercolateQueryDescriptor<T> Document<TDocument>(TDocument document) => Assign(a => a.Document = document);

		/// <summary>
		/// Like the document parameter, but accepts multiple documents.
		/// </summary>
		public PercolateQueryDescriptor<T> Documents<TDocument>(IEnumerable<TDocument> documents) => Assign(a => a.Documents = documents.Cast<object>());
		
		/// <summary>
		/// The id of the document to fetch for percolation.
		/// Can be specified to percolate an existing document instead of providing <see cref="Document{TDocument}"/>
		/// </summary>
		public PercolateQueryDescriptor<T> Id(string id) => Assign(a => a.Id = id);

		/// <summary>
		/// The index the document resides in for percolation.
		/// Can be specified to percolate an existing document instead of providing <see cref="Document{TDocument}"/>
		/// </summary>
		public PercolateQueryDescriptor<T> Index(IndexName index) => Assign(a => a.Index = index);

		/// <summary>
		/// The index the document resides in for percolation.
		/// Can be specified to percolate an existing document instead of providing <see cref="Document{TDocument}"/>
		/// </summary>
		public PercolateQueryDescriptor<T> Index<TDocument>() => Assign(a => a.Index = typeof(TDocument));

		/// <summary>
		/// The type of the document to fetch for percolation.
		/// Can be specified to percolate an existing document instead of providing <see cref="Document{TDocument}"/>
		/// </summary>
		public PercolateQueryDescriptor<T> Type(TypeName type) => Assign(a => a.Type = type);

		/// <summary>
		/// The type of the document to fetch for percolation.
		/// Can be specified to percolate an existing document instead of providing <see cref="Document{TDocument}"/>
		/// </summary>
		public PercolateQueryDescriptor<T> Type<TDocument>() => Assign(a => a.Type = typeof(TDocument));

		/// <summary>
		/// Routing to be used to fetch the document to percolate.
		/// Can be specified to percolate an existing document instead of providing <see cref="Document{TDocument}"/>
		/// </summary>
		public PercolateQueryDescriptor<T> Routing(Routing routing) => Assign(a => a.Routing = routing);

		/// <summary>
		/// Preference to be used to fetch the document to percolate.
		/// Can be specified to percolate an existing document instead of providing <see cref="Document{TDocument}"/>
		/// </summary>
		public PercolateQueryDescriptor<T> Preference(string preference) => Assign(a => a.Preference = preference);

		/// <summary>
		/// The expected version of the document to be fetched for percolation.
		/// Can be specified to percolate an existing document instead of providing <see cref="Document{TDocument}"/>
		/// </summary>
		public PercolateQueryDescriptor<T> Version(long version) => Assign(a => a.Version = version);
	}
}
