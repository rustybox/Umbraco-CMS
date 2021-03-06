using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.AspNetCore.Identity;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models.Identity
{

    /// <summary>
    /// Abstract class for use in Umbraco Identity for users and members
    /// </summary>
    /// <remarks>
    /// <para>
    /// This uses strings for the ID of the user, claims, roles. This is because aspnetcore identity's base store will
    /// not support having an INT user PK and a string role PK with the way they've made the generics. So we will just use
    /// string for both which makes things more flexible anyways for users and members and also if/when we transition to
    /// GUID support
    /// </para>
    /// <para>
    /// This class was originally borrowed from the EF implementation in Identity prior to netcore.
    /// The new IdentityUser in netcore does not have properties such as Claims, Roles and Logins and those are instead
    /// by default managed with their default user store backed by EF which utilizes EF's change tracking to track these values
    /// to a user. We will continue using this approach since it works fine for what we need which does the change tracking of
    /// claims, roles and logins directly on the user model.
    /// </para>
    /// </remarks>
    public abstract class UmbracoIdentityUser : IdentityUser, IRememberBeingDirty
    {
        private string _id;
        private string _email;
        private string _userName;
        private DateTime? _lastLoginDateUtc;
        private bool _emailConfirmed;
        private int _accessFailedCount;
        private string _passwordHash;
        private DateTime? _lastPasswordChangeDateUtc;
        private ObservableCollection<IIdentityUserLogin> _logins;
        private Lazy<IEnumerable<IIdentityUserLogin>> _getLogins;
        private ObservableCollection<IdentityUserRole<string>> _roles;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoIdentityUser"/> class.
        /// </summary>
        public UmbracoIdentityUser()
        {
            // must initialize before setting groups
            _roles = new ObservableCollection<IdentityUserRole<string>>();
            _roles.CollectionChanged += Roles_CollectionChanged;
            Claims = new List<IdentityUserClaim<string>>();
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                BeingDirty.PropertyChanged += value;
            }

            remove
            {
                BeingDirty.PropertyChanged -= value;
            }
        }

        /// <summary>
        /// Gets or sets last login date
        /// </summary>
        public DateTime? LastLoginDateUtc
        {
            get => _lastLoginDateUtc;
            set => BeingDirty.SetPropertyValueAndDetectChanges(value, ref _lastLoginDateUtc, nameof(LastLoginDateUtc));
        }

        /// <summary>
        /// Gets or sets email
        /// </summary>
        public override string Email
        {
            get => _email;
            set => BeingDirty.SetPropertyValueAndDetectChanges(value, ref _email, nameof(Email));
        }

        /// <summary>
        /// Gets or sets a value indicating whether the email is confirmed, default is false
        /// </summary>
        public override bool EmailConfirmed
        {
            get => _emailConfirmed;
            set => BeingDirty.SetPropertyValueAndDetectChanges(value, ref _emailConfirmed, nameof(EmailConfirmed));
        }

        /// <summary>
        /// Gets or sets the salted/hashed form of the user password
        /// </summary>
        public override string PasswordHash
        {
            get => _passwordHash;
            set => BeingDirty.SetPropertyValueAndDetectChanges(value, ref _passwordHash, nameof(PasswordHash));
        }

        /// <summary>
        /// Gets or sets dateTime in UTC when the password was last changed.
        /// </summary>
        public DateTime? LastPasswordChangeDateUtc
        {
            get => _lastPasswordChangeDateUtc;
            set => BeingDirty.SetPropertyValueAndDetectChanges(value, ref _lastPasswordChangeDateUtc, nameof(LastPasswordChangeDateUtc));
        }

        /// <summary>
        /// Gets or sets a value indicating whether is lockout enabled for this user
        /// </summary>
        /// <remarks>
        /// Currently this is always true for users and members
        /// </remarks>
        public override bool LockoutEnabled
        {
            get => true;
            set { }
        }

        /// <summary>
        /// Gets or sets the value to record failures for the purposes of lockout
        /// </summary>
        public override int AccessFailedCount
        {
            get => _accessFailedCount;
            set => BeingDirty.SetPropertyValueAndDetectChanges(value, ref _accessFailedCount, nameof(AccessFailedCount));
        }

        /// <summary>
        /// Gets or sets the user roles collection
        /// </summary>
        public ICollection<IdentityUserRole<string>> Roles
        {
            get => _roles;
            set
            {
                _roles.CollectionChanged -= Roles_CollectionChanged;
                _roles = new ObservableCollection<IdentityUserRole<string>>(value);
                _roles.CollectionChanged += Roles_CollectionChanged;
            }
        }

        /// <summary>
        /// Gets navigation the user claims collection
        /// </summary>
        public ICollection<IdentityUserClaim<string>> Claims { get; }

        /// <summary>
        /// Gets the user logins collection
        /// </summary>
        public ICollection<IIdentityUserLogin> Logins
        {
            get
            {
                // return if it exists
                if (_logins != null)
                {
                    return _logins;
                }

                _logins = new ObservableCollection<IIdentityUserLogin>();

                // if the callback is there and hasn't been created yet then execute it and populate the logins
                if (_getLogins != null && !_getLogins.IsValueCreated)
                {
                    foreach (IIdentityUserLogin l in _getLogins.Value)
                    {
                        _logins.Add(l);
                    }
                }

                // now assign events
                _logins.CollectionChanged += Logins_CollectionChanged;

                return _logins;
            }
        }

        /// <summary>
        /// Gets or sets user ID (Primary Key)
        /// </summary>
        public override string Id
        {
            get => _id;
            set
            {
                _id = value;
                HasIdentity = true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether returns an Id has been set on this object this will be false if the object is new and not persisted to the database
        /// </summary>
        public bool HasIdentity { get; protected set; }

        /// <summary>
        /// Gets or sets user name
        /// </summary>
        public override string UserName
        {
            get => _userName;
            set => BeingDirty.SetPropertyValueAndDetectChanges(value, ref _userName, nameof(UserName));
        }

        /// <summary>
        /// Gets the <see cref="BeingDirty"/> for change tracking
        /// </summary>
        protected BeingDirty BeingDirty { get; } = new BeingDirty();

        /// <inheritdoc />
        public bool IsDirty() => BeingDirty.IsDirty();

        /// <inheritdoc />
        public bool IsPropertyDirty(string propName) => BeingDirty.IsPropertyDirty(propName);

        /// <inheritdoc />
        public IEnumerable<string> GetDirtyProperties() => BeingDirty.GetDirtyProperties();

        /// <inheritdoc />
        public void ResetDirtyProperties() => BeingDirty.ResetDirtyProperties();

        /// <inheritdoc />
        public bool WasDirty() => BeingDirty.WasDirty();

        /// <inheritdoc />
        public bool WasPropertyDirty(string propertyName) => BeingDirty.WasPropertyDirty(propertyName);

        /// <inheritdoc />
        public void ResetWereDirtyProperties() => BeingDirty.ResetWereDirtyProperties();

        /// <inheritdoc />
        public void ResetDirtyProperties(bool rememberDirty) => BeingDirty.ResetDirtyProperties(rememberDirty);

        /// <inheritdoc />
        public IEnumerable<string> GetWereDirtyProperties() => BeingDirty.GetWereDirtyProperties();

        /// <summary>
        /// Disables change tracking.
        /// </summary>
        public void DisableChangeTracking() => BeingDirty.DisableChangeTracking();

        /// <summary>
        /// Enables change tracking.
        /// </summary>
        public void EnableChangeTracking() => BeingDirty.EnableChangeTracking();

        /// <summary>
        /// Adds a role 
        /// </summary>
        /// <param name="role">The role to add</param>
        /// <remarks>
        /// Adding a role this way will not reflect on the user's group's collection or it's allowed sections until the user is persisted
        /// </remarks>
        public void AddRole(string role) => Roles.Add(new IdentityUserRole<string>
        {
            UserId = Id,
            RoleId = role
        });

        /// <summary>
        /// Used to set a lazy call back to populate the user's Login list
        /// </summary>
        /// <param name="callback">The lazy value</param>
        public void SetLoginsCallback(Lazy<IEnumerable<IIdentityUserLogin>> callback) => _getLogins = callback ?? throw new ArgumentNullException(nameof(callback));

        private void Logins_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => BeingDirty.OnPropertyChanged(nameof(Logins));

        private void Roles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => BeingDirty.OnPropertyChanged(nameof(Roles));
    }
}
