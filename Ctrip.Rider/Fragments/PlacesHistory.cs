using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Ctrip.Rider.Adapters;
using Ctrip.Rider.DataModels;
using Ctrip.Rider.EventListeners;
using Ctrip.Rider.Helpers;
using static Android.Support.V7.Widget.RecyclerView;

namespace Ctrip.Rider.Fragments
{
    public class PlacesHistory : Android.Support.V4.App.Fragment
    {
        RecentHistoryListener _historyListener;
        RecyclerView _historyRecycler;

        List<NewTripDetails> _recentSearches;
        HistoryAdapter _historyAdapter;

        private Toolbar _myToolbar;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.places_layout, container, false);
            _myToolbar = view.FindViewById<Toolbar>(Resource.Id.places_toolbar);
            _myToolbar.Title = Resources.GetText(Resource.String.txtHistory);

            RetrieveData();

            _historyRecycler = view.FindViewById<RecyclerView>(Resource.Id.history_recycler);

            return view;
        }

        public void RetrieveData()
        {
            _historyListener = new RecentHistoryListener();
            _historyListener.Create();
            _historyListener.HistoryRetrieved += HistoryListener_HistoryRetrieved;
        }

        private void HistoryListener_HistoryRetrieved(object sender, RecentHistoryListener.RecentTripEventArgs e)
        {
            _recentSearches = e.RecentTripList;
            SetUpRecyclerView();
        }

        private void SetUpRecyclerView()
        {
            LayoutManager layoutManager = new LinearLayoutManager(_historyRecycler.Context);
            _historyAdapter = new HistoryAdapter(_recentSearches);
            _historyRecycler.SetAdapter(_historyAdapter);
            _historyRecycler.SetLayoutManager(layoutManager);

            ItemDecoration itemDecoration = new RecyclerItemDecor(Application.Context.GetDrawable(Resource.Drawable.divider));
            _historyRecycler.AddItemDecoration(itemDecoration);
            _historyAdapter.ItemClick += HistoryAdapter_ItemClick;
        }

        private void HistoryAdapter_ItemClick(object sender, int e)
        {

        }
    }
}