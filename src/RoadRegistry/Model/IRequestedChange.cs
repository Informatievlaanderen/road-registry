﻿namespace RoadRegistry.Model
{
    public interface IRequestedChange
    {
        IVerifiedChange Verify(ChangeContext context);

        void TranslateTo(Messages.AcceptedChange message);
        void TranslateTo(Messages.RejectedChange message);
    }

//    public interface IVerifiableChange
//    {
//        IVerifiedChange Verify(ChangeContext context);
//    }
}
