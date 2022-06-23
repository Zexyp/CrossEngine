using System;
using static BulletSharp.UnsafeNativeMethods;

namespace BulletSharp
{
	public class GImpactCollisionAlgorithm : ActivatingCollisionAlgorithm
	{
		public class CreateFunc : CollisionAlgorithmCreateFunc
		{
			internal CreateFunc(IntPtr native, BulletObject owner)
				: base(ConstructionInfo.Null)
			{
				InitializeSubObject(native, owner);
			}

			public CreateFunc()
				: base(ConstructionInfo.Null)
			{
				IntPtr native = btGImpactCollisionAlgorithm_CreateFunc_new();
				InitializeUserOwned(native);
			}

			public override CollisionAlgorithm CreateCollisionAlgorithm(CollisionAlgorithmConstructionInfo __unnamed0,
				CollisionObjectWrapper body0Wrap, CollisionObjectWrapper body1Wrap)
			{
				return new GImpactCollisionAlgorithm(btCollisionAlgorithmCreateFunc_CreateCollisionAlgorithm(
					Native, __unnamed0.Native, body0Wrap.Native, body1Wrap.Native), __unnamed0.Dispatcher);
			}
		}

		internal GImpactCollisionAlgorithm(IntPtr native, BulletObject owner)
		{
			InitializeSubObject(native, owner);
		}

		public GImpactCollisionAlgorithm(CollisionAlgorithmConstructionInfo constructionInfo, CollisionObjectWrapper body0Wrap,
			CollisionObjectWrapper body1Wrap)
		{
			IntPtr native = btGImpactCollisionAlgorithm_new(constructionInfo.Native, body0Wrap.Native,
				body1Wrap.Native);
			InitializeUserOwned(native);
		}

		public void GImpactVsCompoundShape(CollisionObjectWrapper body0Wrap, CollisionObjectWrapper body1Wrap,
			GImpactShapeInterface shape0, CompoundShape shape1, bool swapped)
		{
			btGImpactCollisionAlgorithm_gimpact_vs_compoundshape(Native, body0Wrap.Native,
				body1Wrap.Native, shape0.Native, shape1.Native, swapped);
		}

		public void GImpactVsConcave(CollisionObjectWrapper body0Wrap, CollisionObjectWrapper body1Wrap,
			GImpactShapeInterface shape0, ConcaveShape shape1, bool swapped)
		{
			btGImpactCollisionAlgorithm_gimpact_vs_concave(Native, body0Wrap.Native,
				body1Wrap.Native, shape0.Native, shape1.Native, swapped);
		}

		public void GImpactVsGImpact(CollisionObjectWrapper body0Wrap, CollisionObjectWrapper body1Wrap,
			GImpactShapeInterface shape0, GImpactShapeInterface shape1)
		{
			btGImpactCollisionAlgorithm_gimpact_vs_gimpact(Native, body0Wrap.Native,
				body1Wrap.Native, shape0.Native, shape1.Native);
		}

		public void GImpactVsShape(CollisionObjectWrapper body0Wrap, CollisionObjectWrapper body1Wrap,
			GImpactShapeInterface shape0, CollisionShape shape1, bool swapped)
		{
			btGImpactCollisionAlgorithm_gimpact_vs_shape(Native, body0Wrap.Native,
				body1Wrap.Native, shape0.Native, shape1.Native, swapped);
		}

		public ManifoldResult InternalGetResultOut()
		{
			return new ManifoldResult(btGImpactCollisionAlgorithm_internalGetResultOut(Native), this);
		}

		public static void RegisterAlgorithm(CollisionDispatcher dispatcher)
		{
			btGImpactCollisionAlgorithm_registerAlgorithm(dispatcher.Native);
		}

		public int Face0
		{
			get => btGImpactCollisionAlgorithm_getFace0(Native);
			set => btGImpactCollisionAlgorithm_setFace0(Native, value);
		}

		public int Face1
		{
			get => btGImpactCollisionAlgorithm_getFace1(Native);
			set => btGImpactCollisionAlgorithm_setFace1(Native, value);
		}

		public int Part0
		{
			get => btGImpactCollisionAlgorithm_getPart0(Native);
			set => btGImpactCollisionAlgorithm_setPart0(Native, value);
		}

		public int Part1
		{
			get => btGImpactCollisionAlgorithm_getPart1(Native);
			set => btGImpactCollisionAlgorithm_setPart1(Native, value);
		}
	}
}
