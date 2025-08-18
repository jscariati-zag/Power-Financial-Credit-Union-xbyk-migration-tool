using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using URLRedirection;

[assembly: RegisterObjectType(typeof(RedirectionTableInfo), RedirectionTableInfo.OBJECT_TYPE)]

namespace URLRedirection
{
    /// <summary>
    /// Data container class for <see cref="RedirectionTableInfo"/>.
    /// </summary>
    public partial class RedirectionTableInfo : AbstractInfo<RedirectionTableInfo, IInfoProvider<RedirectionTableInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "urlredirection.redirectiontable";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<RedirectionTableInfo>), OBJECT_TYPE, "URLRedirection.RedirectionTable", "RedirectionTableID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>()
            {
                new ObjectDependency("RedirectionSiteID", "cms.websitechannel", ObjectDependencyEnum.RequiredHasDefault),
            },
        };


        /// <summary>
        /// Redirection table ID.
        /// </summary>
        [DatabaseField]
        public virtual int RedirectionTableID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(RedirectionTableID)), 0);
            set => SetValue(nameof(RedirectionTableID), value);
        }


        /// <summary>
        /// Redirection enabled.
        /// </summary>
        [DatabaseField]
        public virtual bool RedirectionEnabled
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(RedirectionEnabled)), true);
            set => SetValue(nameof(RedirectionEnabled), value);
        }


        /// <summary>
        /// Redirection original URL.
        /// </summary>
        [DatabaseField]
        public virtual string RedirectionOriginalURL
        {
            get => ValidationHelper.GetString(GetValue(nameof(RedirectionOriginalURL)), String.Empty);
            set => SetValue(nameof(RedirectionOriginalURL), value);
        }


        /// <summary>
        /// Redirection target URL.
        /// </summary>
        [DatabaseField]
        public virtual string RedirectionTargetURL
        {
            get => ValidationHelper.GetString(GetValue(nameof(RedirectionTargetURL)), String.Empty);
            set => SetValue(nameof(RedirectionTargetURL), value);
        }


        /// <summary>
        /// Redirection site ID.
        /// </summary>
        [DatabaseField]
        public virtual int RedirectionSiteID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(RedirectionSiteID)), 0);
            set => SetValue(nameof(RedirectionSiteID), value);
        }


        /// <summary>
        /// Redirection type.
        /// </summary>
        [DatabaseField]
        public virtual string RedirectionType
        {
            get => ValidationHelper.GetString(GetValue(nameof(RedirectionType)), String.Empty);
            set => SetValue(nameof(RedirectionType), value);
        }


        /// <summary>
        /// Redirection created when.
        /// </summary>
        [DatabaseField]
        public virtual DateTime RedirectionCreatedWhen
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(RedirectionCreatedWhen)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(RedirectionCreatedWhen), value, DateTimeHelper.ZERO_TIME);
        }


        /// <summary>
        /// Redirection created by user ID.
        /// </summary>
        [DatabaseField]
        public virtual int RedirectionCreatedByUserID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(RedirectionCreatedByUserID)), 0);
            set => SetValue(nameof(RedirectionCreatedByUserID), value, 0);
        }


        /// <summary>
        /// Redirection modified when.
        /// </summary>
        [DatabaseField]
        public virtual DateTime RedirectionModifiedWhen
        {
            get => ValidationHelper.GetDateTime(GetValue(nameof(RedirectionModifiedWhen)), DateTimeHelper.ZERO_TIME);
            set => SetValue(nameof(RedirectionModifiedWhen), value, DateTimeHelper.ZERO_TIME);
        }


        /// <summary>
        /// Redirection modified by user ID.
        /// </summary>
        [DatabaseField]
        public virtual int RedirectionModifiedByUserID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(RedirectionModifiedByUserID)), 0);
            set => SetValue(nameof(RedirectionModifiedByUserID), value, 0);
        }


        /// <summary>
        /// Redirection migrated.
        /// </summary>
        [DatabaseField]
        public virtual bool RedirectionMigrated
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(RedirectionMigrated)), true);
            set => SetValue(nameof(RedirectionMigrated), value);
        }


        /// <summary>
        /// Redirection guid.
        /// </summary>
        [DatabaseField]
        public virtual Guid RedirectionGuid
        {
            get => ValidationHelper.GetGuid(GetValue(nameof(RedirectionGuid)), Guid.Empty);
            set => SetValue(nameof(RedirectionGuid), value, Guid.Empty);
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            Provider.Delete(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            Provider.Set(this);
        }


        /// <summary>
        /// Creates an empty instance of the <see cref="RedirectionTableInfo"/> class.
        /// </summary>
        public RedirectionTableInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="RedirectionTableInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public RedirectionTableInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}