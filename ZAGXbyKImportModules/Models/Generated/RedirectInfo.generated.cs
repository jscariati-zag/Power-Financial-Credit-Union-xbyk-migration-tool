using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using Redirects;

[assembly: RegisterObjectType(typeof(RedirectInfo), RedirectInfo.OBJECT_TYPE)]

namespace Redirects
{
    /// <summary>
    /// Data container class for <see cref="RedirectInfo"/>.
    /// </summary>
    public partial class RedirectInfo : AbstractInfo<RedirectInfo, IInfoProvider<RedirectInfo>>, IInfoWithId
    {
        /// <summary>
        /// Object type.
        /// </summary>
        public const string OBJECT_TYPE = "redirects.redirect";


        /// <summary>
        /// Type information.
        /// </summary>
#warning "You will need to configure the type info."
        public static readonly ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(IInfoProvider<RedirectInfo>), OBJECT_TYPE, "Redirects.Redirect", "RedirectID", null, null, null, null, null, null, null)
        {
            TouchCacheDependencies = true,
        };


        /// <summary>
        /// Redirect ID.
        /// </summary>
        [DatabaseField]
        public virtual int RedirectID
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(RedirectID)), 0);
            set => SetValue(nameof(RedirectID), value);
        }


        /// <summary>
        /// Redirect enabled.
        /// </summary>
        [DatabaseField]
        public virtual bool RedirectEnabled
        {
            get => ValidationHelper.GetBoolean(GetValue(nameof(RedirectEnabled)), true);
            set => SetValue(nameof(RedirectEnabled), value);
        }


        /// <summary>
        /// Redirect channel id.
        /// </summary>
        [DatabaseField]
        public virtual int RedirectChannelId
        {
            get => ValidationHelper.GetInteger(GetValue(nameof(RedirectChannelId)), 0);
            set => SetValue(nameof(RedirectChannelId), value);
        }


        /// <summary>
        /// Redirect original url.
        /// </summary>
        [DatabaseField]
        public virtual string RedirectOriginalUrl
        {
            get => ValidationHelper.GetString(GetValue(nameof(RedirectOriginalUrl)), String.Empty);
            set => SetValue(nameof(RedirectOriginalUrl), value);
        }


        /// <summary>
        /// Redirect target url.
        /// </summary>
        [DatabaseField]
        public virtual string RedirectTargetUrl
        {
            get => ValidationHelper.GetString(GetValue(nameof(RedirectTargetUrl)), String.Empty);
            set => SetValue(nameof(RedirectTargetUrl), value);
        }


        /// <summary>
        /// Redirect type.
        /// </summary>
        [DatabaseField]
        public virtual string RedirectType
        {
            get => ValidationHelper.GetString(GetValue(nameof(RedirectType)), String.Empty);
            set => SetValue(nameof(RedirectType), value);
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
        /// Creates an empty instance of the <see cref="RedirectInfo"/> class.
        /// </summary>
        public RedirectInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new instances of the <see cref="RedirectInfo"/> class from the given <see cref="DataRow"/>.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public RedirectInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }
    }
}