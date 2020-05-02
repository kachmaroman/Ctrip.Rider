using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Globalization;
using Ctrip.Rider.DataModels;

namespace Ctrip.Rider.Adapters
{
	public class HistoryAdapter : RecyclerView.Adapter
    {
        public event EventHandler<int> ItemClick;

        readonly List<NewTripDetails> _searchList;

        public HistoryAdapter(List<NewTripDetails> data)
        {
            _searchList = data;
            NotifyDataSetChanged();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
	        View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.history_adapterview, parent, false);

            return new HistoryAdapterViewHolder(itemView, OnClick);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
	        if (viewHolder is HistoryAdapterViewHolder holder)
	        {
		        string time = _searchList[position].Timestamp.ToString(CultureInfo.InvariantCulture);
		        string fare = $"₴ {_searchList[position].EstimateFare}";

		        holder.PlaceTextView.Text = $"{time} - {fare}";
            }
        }

        public override int ItemCount => _searchList.Count;

        void OnClick(int position) => ItemClick?.Invoke(this, position);
    }

    public class HistoryAdapterViewHolder : RecyclerView.ViewHolder
    {
        public TextView PlaceTextView { get; set; }

        public HistoryAdapterViewHolder(View itemView, Action<int> clickListener) : base(itemView)
        {
            PlaceTextView = itemView.FindViewById<TextView>(Resource.Id.history_tv);
            
            itemView.Click += (sender, pos) => clickListener(LayoutPosition);
        }
    }
}