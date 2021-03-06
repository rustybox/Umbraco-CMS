using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NUnit.Framework;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Common.AspNetCore;
using Umbraco.Web.Common.ModelBinders;
using Umbraco.Web.Models;

namespace Umbraco.Tests.UnitTests.Umbraco.Web.Common.Views
{
    [TestFixture]
    public class UmbracoViewPageTests
    {
        #region RenderModel To ...
        [Test]
        public void RenderModel_To_RenderModel()
        {
            var content = new ContentType1(null);
            var model = new ContentModel(content);
            var view = new RenderModelTestPage();
            var viewData = GetViewDataDictionary<ContentModel>(model);
            view.ViewData = viewData;

            Assert.AreSame(model, view.Model);
        }

        [Test]
        public void RenderModel_ContentType1_To_ContentType1()
        {
            var content = new ContentType1(null);
            var view = new ContentType1TestPage();
            var viewData = GetViewDataDictionary<ContentType1>(content);

            view.ViewData = viewData;

            Assert.IsInstanceOf<ContentType1>(view.Model);
        }

        [Test]
        public void RenderModel_ContentType2_To_ContentType1()
        {
            var content = new ContentType2(null);
            var view = new ContentType1TestPage();

            var viewData = GetViewDataDictionary<ContentType1>(content);
            view.ViewData = viewData;

            Assert.IsInstanceOf<ContentType1>(view.Model);
        }

        [Test]
        public void RenderModel_ContentType1_To_ContentType2()
        {
            var content = new ContentType1(null);
            var model = new ContentModel(content);
            var view = new ContentType2TestPage();
            var viewData = GetViewDataDictionary(model);

            Assert.ThrowsAsync<ModelBindingException>(async () => await view.SetViewDataAsyncX(viewData));
        }

        [Test]
        public void RenderModel_ContentType1_To_RenderModelOf_ContentType1()
        {
            var content = new ContentType1(null);
            var model = new ContentModel<ContentType1>(content);
            var view = new RenderModelOfContentType1TestPage();
            var viewData = GetViewDataDictionary<ContentModel<ContentType1>>(model);

            view.ViewData = viewData;

            Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
            Assert.IsInstanceOf<ContentType1>(view.Model.Content);
        }

        [Test]
        public void RenderModel_ContentType2_To_RenderModelOf_ContentType1()
        {
            var content = new ContentType2(null);
            var model = new ContentModel<ContentType1>(content);
            var view = new RenderModelOfContentType1TestPage();
            var viewData = GetViewDataDictionary<ContentModel<ContentType1>>(model);
            view.ViewData = viewData;

            Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
            Assert.IsInstanceOf<ContentType2>(view.Model.Content);
        }

        [Test]
        public void RenderModel_ContentType1_To_RenderModelOf_ContentType2()
        {
            var content = new ContentType1(null);
            var model = new ContentModel(content);
            var view = new RenderModelOfContentType2TestPage();
            var viewData = GetViewDataDictionary(model);

            Assert.ThrowsAsync<ModelBindingException>(async () => await view.SetViewDataAsyncX(viewData));
        }

        #endregion

        #region RenderModelOf To ...

        [Test]
        public void RenderModelOf_ContentType1_To_RenderModel()
        {
            var content = new ContentType1(null);
            var model = new ContentModel<ContentType1>(content);
            var view = new RenderModelTestPage();
            var viewData = GetViewDataDictionary<ContentModel>(model);

            view.ViewData = viewData;

            Assert.AreSame(model, view.Model);
        }

        [Test]
        public async Task RenderModelOf_ContentType1_To_ContentType1()
        {
            var content = new ContentType1(null);
            var model = new ContentModel<ContentType1>(content);
            var view = new ContentType1TestPage();
            var viewData = GetViewDataDictionary<ContentModel<ContentType1>>(model);

            await view.SetViewDataAsyncX(viewData);

            Assert.IsInstanceOf<ContentType1>(view.Model);
        }

        [Test]
        public async Task RenderModelOf_ContentType2_To_ContentType1()
        {
            var content = new ContentType2(null);
            var model = new ContentModel<ContentType2>(content);
            var view = new ContentType1TestPage();
            var viewData =  new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model =  model
            };

            await view.SetViewDataAsyncX(viewData);

            Assert.IsInstanceOf<ContentType1>(view.Model);
        }

        [Test]
        public async Task RenderModelOf_ContentType1_To_ContentType2()
        {

            var content = new ContentType1(null);
            var model = new ContentModel<ContentType1>(content);
            var view = new ContentType2TestPage();
            var viewData = GetViewDataDictionary(model);

            Assert.ThrowsAsync<ModelBindingException>(async () => await view.SetViewDataAsyncX(viewData));
        }

        [Test]
        public void RenderModelOf_ContentType1_To_RenderModelOf_ContentType1()
        {
            var content = new ContentType1(null);
            var model = new ContentModel<ContentType1>(content);
            var view = new RenderModelOfContentType1TestPage();
            var viewData = GetViewDataDictionary<ContentModel<ContentType1>>(model);

            view.ViewData = viewData;

            Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
            Assert.IsInstanceOf<ContentType1>(view.Model.Content);
        }

        [Test]
        public async Task RenderModelOf_ContentType2_To_RenderModelOf_ContentType1()
        {
            var content = new ContentType2(null);
            var model = new ContentModel<ContentType1>(content);
            var view = new RenderModelOfContentType1TestPage();
            var viewData = GetViewDataDictionary<ContentModel<ContentType1>>(model);

            await view.SetViewDataAsyncX(viewData);

            Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
            Assert.IsInstanceOf<ContentType2>(view.Model.Content);
        }

        [Test]
        public void RenderModelOf_ContentType1_To_RenderModelOf_ContentType2()
        {
            var content = new ContentType1(null);
            var model = new ContentModel<ContentType1>(content);
            var view = new RenderModelOfContentType2TestPage();
            var viewData = GetViewDataDictionary(model);

            Assert.ThrowsAsync<ModelBindingException>(async () => await view.SetViewDataAsyncX(viewData));
        }

        #endregion

        #region ContentType To ...

        [Test]
        public async Task ContentType1_To_RenderModel()
        {
            var content = new ContentType1(null);
            var view = new RenderModelTestPage();

            var viewData = GetViewDataDictionary<ContentType1>(content);

            await view.SetViewDataAsyncX(viewData);

            Assert.IsInstanceOf<ContentModel>(view.Model);
        }

        [Test]
        public async Task ContentType1_To_RenderModelOf_ContentType1()
        {
            var content = new ContentType1(null);
            var view = new RenderModelOfContentType1TestPage();

            var viewData = GetViewDataDictionary<ContentType1>(content);
            await view.SetViewDataAsyncX(viewData);

            Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
            Assert.IsInstanceOf<ContentType1>(view.Model.Content);
        }

        [Test]
        public async Task ContentType2_To_RenderModelOf_ContentType1()
        {
            // Same as above but with ContentModel<ContentType2>
            var content = new ContentType2(null);
            var view = new RenderModelOfContentType1TestPage();
            var viewData = GetViewDataDictionary<ContentType2>(content);

            await view.SetViewDataAsyncX(viewData);

            Assert.IsInstanceOf<ContentModel<ContentType1>>(view.Model);
            Assert.IsInstanceOf<ContentType1>(view.Model.Content);
        }

        [Test]
        public void ContentType1_To_RenderModelOf_ContentType2()
        {
            var content = new ContentType1(null);
            var view = new RenderModelOfContentType2TestPage();
            var viewData = GetViewDataDictionary(content);

            Assert.ThrowsAsync<ModelBindingException>(async () => await view.SetViewDataAsyncX(viewData));
        }

        [Test]
        public async Task ContentType1_To_ContentType1()
        {
            var content = new ContentType1(null);
            var view = new ContentType1TestPage();
            var viewData = GetViewDataDictionary<ContentType1>(content);

            await view.SetViewDataAsyncX(viewData);

            Assert.IsInstanceOf<ContentType1>(view.Model);
        }

        [Test]
        public void ContentType1_To_ContentType2()
        {
            var content = new ContentType1(null);
            var view = new ContentType2TestPage();
            var viewData = GetViewDataDictionary(content);

            Assert.ThrowsAsync<ModelBindingException>(async () => await view.SetViewDataAsyncX(viewData));
        }

        [Test]
        public async Task ContentType2_To_ContentType1()
        {
            var content = new ContentType2(null);
            var view = new ContentType1TestPage();
            var viewData = GetViewDataDictionary(content);

            await view.SetViewDataAsyncX(viewData);

            Assert.IsInstanceOf<ContentType1>(view.Model);
        }

        #endregion

        #region Test helpers methods

        private ViewDataDictionary<T> GetViewDataDictionary<T>(object model)
        {
            var sourceViewDataDictionary = new ViewDataDictionary<T>(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            return new ViewDataDictionary<T>(sourceViewDataDictionary, model);
        }

        private ViewDataDictionary GetViewDataDictionary(object model)
        {
            var sourceViewDataDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary());
            return new ViewDataDictionary(sourceViewDataDictionary)
            {
                Model = model
            };
        }

        #endregion

        #region Test elements

        public class ContentType1 : PublishedContentWrapped
        {
            public ContentType1(IPublishedContent content) : base(content) {}
        }

        public class ContentType2 : ContentType1
        {
            public ContentType2(IPublishedContent content) : base(content) { }
        }

        public class TestPage<TModel> : UmbracoViewPage<TModel>
        {
            public override Task ExecuteAsync()
            {
                throw new NotImplementedException();
            }

            public async Task SetViewDataAsyncX(ViewDataDictionary viewData)
            {
                await SetViewDataAsync(viewData);
            }
        }

        public class RenderModelTestPage : TestPage<ContentModel>
        { }

        public class ContentType1TestPage : TestPage<ContentType1>
        { }

        public class ContentType2TestPage : TestPage<ContentType2>
        { }

        public class RenderModelOfContentType1TestPage : TestPage<ContentModel<ContentType1>>
        { }

        public class RenderModelOfContentType2TestPage : TestPage<ContentModel<ContentType2>>
        { }

        #endregion
    }
}
