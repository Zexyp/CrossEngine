﻿using System;
using static BulletSharp.UnsafeNativeMethods;

namespace BulletSharp
{
	public class ConstraintSolverPoolMultiThreaded : ConstraintSolver
	{
		public ConstraintSolverPoolMultiThreaded(int numSolvers)
		{
			IntPtr native = btConstraintSolverPoolMt_new(numSolvers);
			InitializeUserOwned(native);
		}
	}

	public class DiscreteDynamicsWorldMultiThreaded : DiscreteDynamicsWorld
	{
		public DiscreteDynamicsWorldMultiThreaded(Dispatcher dispatcher, BroadphaseInterface pairCache,
			ConstraintSolverPoolMultiThreaded constraintSolver, ConstraintSolver constraintSolverMultiThreaded,
			CollisionConfiguration collisionConfiguration)
		{
			IntPtr native = btDiscreteDynamicsWorldMt_new(
				dispatcher != null ? dispatcher.Native : IntPtr.Zero,
				pairCache != null ? pairCache.Native : IntPtr.Zero,
				constraintSolver != null ? constraintSolver.Native : IntPtr.Zero,
				constraintSolverMultiThreaded != null ? constraintSolverMultiThreaded.Native : IntPtr.Zero,
				collisionConfiguration != null ? collisionConfiguration.Native : IntPtr.Zero);
			InitializeUserOwned(native);
			InitializeMembers(dispatcher, pairCache, constraintSolver);
		}
	}
}
