using Android.Content;
using Android.Content.Res;
using Android.Runtime;
using Android.Util;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AndroidCustomViewSample
{
    public class AnchorLayout : ViewGroup
    {
        private const GravityFlags defaultChildGravity = GravityFlags.Top | GravityFlags.Left;

        public AnchorLayout(Context context) : this(context, null)
        {
        }

        public AnchorLayout(Context context, IAttributeSet attrs) : this(context, attrs, 0)
        {
        }

        public AnchorLayout(Context context, IAttributeSet attrs, int defStyleAttr) : this(context, attrs, defStyleAttr, 0)
        {
        }

        public AnchorLayout(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

        // Xamarin.Android固有のコンストラクター
        public AnchorLayout(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            int width = MeasureSpec.GetSize(widthMeasureSpec);
            int height = MeasureSpec.GetSize(heightMeasureSpec);
            MeasureSpecMode widthMode = MeasureSpec.GetMode(widthMeasureSpec);
            MeasureSpecMode heightMode = MeasureSpec.GetMode(heightMeasureSpec);

            int maxChildWidth = 0;
            int maxChildHeight = 0;

            for (int i = 0; i < ChildCount; i++)
            {
                View child = GetChildAt(i);
                if (child.Visibility == ViewStates.Gone)
                {
                    continue;
                }

                ViewGroup.LayoutParams childLayoutParameters = child.LayoutParameters;

                child.Measure(
                    makeChildMeasureSpec(child, width, widthMode, childLayoutParameters.Width),
                    makeChildMeasureSpec(child, height, heightMode, childLayoutParameters.Height)
                );

                maxChildWidth = Math.Max(maxChildWidth, child.MeasuredWidth);
                maxChildHeight = Math.Max(maxChildHeight, child.MeasuredHeight);
            }

            int contentWidth = widthMode == MeasureSpecMode.Exactly ? width : maxChildWidth;
            int contentHeight = heightMode == MeasureSpecMode.Exactly ? height : maxChildHeight;

            SetMeasuredDimension(contentWidth, contentHeight);
        }

        private int makeChildMeasureSpec(View child, int parentSize, MeasureSpecMode parentMode, int childLayoutParameterSize)
        {
            int size;
            MeasureSpecMode mode;

            switch (parentMode)
            {
                case MeasureSpecMode.AtMost:
                    switch (childLayoutParameterSize)
                    {
                        case ViewGroup.LayoutParams.MatchParent:
                            size = parentSize;
                            mode = MeasureSpecMode.Exactly;
                            break;
                        case ViewGroup.LayoutParams.WrapContent:
                            size = parentSize;
                            mode = MeasureSpecMode.AtMost;
                            break;
                        default:
                            size = childLayoutParameterSize;
                            mode = MeasureSpecMode.Exactly;
                            break;
                    }
                    break;
                case MeasureSpecMode.Unspecified:
                    switch (childLayoutParameterSize)
                    {
                        case ViewGroup.LayoutParams.MatchParent:
                            size = parentSize;
                            mode = MeasureSpecMode.Exactly;
                            break;
                        case ViewGroup.LayoutParams.WrapContent:
                            size = parentSize;
                            mode = MeasureSpecMode.Unspecified;
                            break;
                        default:
                            size = childLayoutParameterSize;
                            mode = MeasureSpecMode.Exactly;
                            break;
                    }
                    break;
                case MeasureSpecMode.Exactly:
                default:
                    switch (childLayoutParameterSize)
                    {
                        case ViewGroup.LayoutParams.MatchParent:
                            size = parentSize;
                            mode = MeasureSpecMode.Exactly;
                            break;
                        case ViewGroup.LayoutParams.WrapContent:
                            size = parentSize;
                            mode = MeasureSpecMode.AtMost;
                            break;
                        default:
                            size = Math.Min(parentSize, childLayoutParameterSize);
                            mode = MeasureSpecMode.Exactly;
                            break;
                    }
                    break;
            }

            return MeasureSpec.MakeMeasureSpec(size, mode);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            int parentTop = PaddingTop;
            int parentBottom = b - t - PaddingBottom;
            int parentLeft = PaddingLeft;
            int parentRight = r - l - PaddingRight;
            var laidout = new List<View>();

            for (int i = 0; i < ChildCount; i++)
            {
                View child = GetChildAt(i);
                if (child.Visibility == ViewStates.Gone)
                {
                    continue;
                }

                int width = child.MeasuredWidth;
                int height = child.MeasuredHeight;

                var layoutParams = child.LayoutParameters as LayoutParams;

                GravityFlags gravity = layoutParams?.Gravity ?? GravityFlags.NoGravity;
                if (gravity == GravityFlags.NoGravity)
                {
                    gravity = defaultChildGravity;
                }

                View anchorView = laidout.Where(x => x.Id == layoutParams?.AnchorId).FirstOrDefault();
                GravityFlags anchorGravity = layoutParams?.AnchorGravity ?? GravityFlags.NoGravity;

                int left;
                if (anchorView != null)
                {
                    left = calculateLayoutPositionWithAnchor(
                        anchorView.Left,
                        anchorView.Right,
                        width,
                        convertRelativeHorizontalGravity(gravity),
                        convertRelativeHorizontalGravity(anchorGravity)
                    );
                }
                else
                {
                    left = calculateLayoutPosition(
                        parentLeft,
                        parentRight,
                        width,
                        convertRelativeHorizontalGravity(gravity)
                    );
                }

                int top;
                if (anchorView != null)
                {
                    top = calculateLayoutPositionWithAnchor(
                        anchorView.Top,
                        anchorView.Bottom,
                        height,
                        convertRelativeVerticalGravity(gravity),
                        convertRelativeVerticalGravity(anchorGravity)
                    );
                }
                else
                {
                    top = calculateLayoutPosition(
                        parentTop,
                        parentBottom,
                        height,
                        convertRelativeVerticalGravity(gravity)
                    );
                }

                child.Layout(left, top, left + width, top + height);

                laidout.Add(child);
            }
        }

        private GravityFlags convertRelativeHorizontalGravity(GravityFlags gravity)
        {
            GravityFlags absoluteGravity = Gravity.GetAbsoluteGravity(gravity, (GravityFlags)LayoutDirection);
            switch (absoluteGravity & GravityFlags.HorizontalGravityMask)
            {
                case GravityFlags.CenterHorizontal:
                    return GravityFlags.Center;
                case GravityFlags.Right:
                    return GravityFlags.End;
                case GravityFlags.Left:
                default:
                    return GravityFlags.Start;
            }
        }

        private GravityFlags convertRelativeVerticalGravity(GravityFlags gravity)
        {
            switch (gravity & GravityFlags.VerticalGravityMask)
            {
                case GravityFlags.CenterVertical:
                    return GravityFlags.Center;
                case GravityFlags.Bottom:
                    return GravityFlags.End;
                case GravityFlags.Top:
                default:
                    return GravityFlags.Start;
            }
        }

        private int calculateLayoutPosition(int parentStart, int parentEnd, int size, GravityFlags gravity)
        {
            switch (gravity)
            {
                case GravityFlags.Center:
                    return parentStart + (parentEnd - parentStart - size) / 2;
                case GravityFlags.End:
                    return parentEnd - size;
                case GravityFlags.Start:
                default:
                    return parentStart;
            }
        }

        private int calculateLayoutPositionWithAnchor(int anchorStart, int anchorEnd, int size, GravityFlags gravity, GravityFlags anchorGravity)
        {
            int basePoint;
            switch (anchorGravity)
            {
                case GravityFlags.Center:
                    basePoint = (anchorStart + anchorEnd) / 2;
                    break;
                case GravityFlags.End:
                    basePoint = anchorEnd;
                    break;
                case GravityFlags.Start:
                default:
                    basePoint = anchorStart;
                    break;
            }
            switch (gravity)
            {
                case GravityFlags.Center:
                    return basePoint - size / 2;
                case GravityFlags.End:
                    return basePoint;
                case GravityFlags.Start:
                default:
                    return basePoint - size;
            }
        }

        /*
         * LayoutParams
         */
        protected override ViewGroup.LayoutParams GenerateDefaultLayoutParams()
        {
            return new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);
        }

        public override ViewGroup.LayoutParams GenerateLayoutParams(IAttributeSet attrs)
        {
            return new LayoutParams(Context, attrs);
        }

        protected override ViewGroup.LayoutParams GenerateLayoutParams(ViewGroup.LayoutParams p)
        {
            if (p is LayoutParams layoutParams)
            {
                return new LayoutParams(layoutParams);
            }
            return new LayoutParams(p);
        }

        protected override bool CheckLayoutParams(ViewGroup.LayoutParams p)
        {
            return p is LayoutParams;
        }

        public new class LayoutParams : ViewGroup.LayoutParams
        {
            public GravityFlags Gravity { get; } = GravityFlags.NoGravity;
            public GravityFlags AnchorGravity { get; } = GravityFlags.NoGravity;
            public int AnchorId { get; }

            public LayoutParams(Context c, IAttributeSet attrs) : base(c, attrs)
            {
                TypedArray a = c.ObtainStyledAttributes(attrs, Resource.Styleable.AnchorLayout_LayoutParams);
                Gravity = (GravityFlags)a.GetInt(Resource.Styleable.AnchorLayout_LayoutParams_android_layout_gravity, (int)GravityFlags.NoGravity);
                AnchorId = a.GetResourceId(Resource.Styleable.AnchorLayout_LayoutParams_layout_targetAnchor, 0);
                AnchorGravity = (GravityFlags)a.GetInt(Resource.Styleable.AnchorLayout_LayoutParams_layout_targetAnchorGravity, (int)GravityFlags.NoGravity);
                a.Recycle();
            }

            public LayoutParams(ViewGroup.LayoutParams source) : base(source)
            {
            }

            public LayoutParams(LayoutParams source) : base(source)
            {
                Gravity = source.Gravity;
                AnchorId = source.AnchorId;
                AnchorGravity = source.AnchorGravity;
            }

            public LayoutParams(int width, int height) : base(width, height)
            {
            }

            public LayoutParams(int width, int height, GravityFlags gravity) : base(width, height)
            {
                Gravity = gravity;
            }

            public LayoutParams(int width, int height, GravityFlags gravity, int anchorId, GravityFlags anchorGravity) : base(width, height)
            {
                Gravity = gravity;
                AnchorId = anchorId;
                AnchorGravity = anchorGravity;
            }

            // Xamarin.Android固有のコンストラクター
            protected LayoutParams(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
            {
            }
        }
    }
}