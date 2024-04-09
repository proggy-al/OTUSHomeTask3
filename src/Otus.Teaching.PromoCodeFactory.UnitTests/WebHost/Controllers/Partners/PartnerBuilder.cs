using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Otus.Teaching.PromoCodeFactory.UnitTests.WebHost.Controllers.Partners
{
    
    public class PartnerBuilder
    {
        private Guid _partnerId;

        private int _numberIssuedPromoCodes;

        private bool _isActive;

        private ICollection<PartnerPromoCodeLimit> _partnerLimits;

        public PartnerBuilder()
        {
            
        }

        public PartnerBuilder WithCreatedParthnerId(Guid Id)
        {
            _partnerId = Id;
            return this;
        }

        public PartnerBuilder WithCreatedNumberIssuedPromoCodes(int numberIssuedPromoCodes)
        {
            _numberIssuedPromoCodes = numberIssuedPromoCodes;
            return this;
        }

        public PartnerBuilder WithCreatedIsActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public PartnerBuilder WithCreatedPartnerLimits(List<PartnerPromoCodeLimit> partnerLimits)
        {
            _partnerLimits = partnerLimits;
            return this;
        }

        public Partner Build()
        {
            return new Partner
            {
                Id = _partnerId,
                IsActive = _isActive,
                NumberIssuedPromoCodes = _numberIssuedPromoCodes,
                PartnerLimits = _partnerLimits
            };
        }
    }
}
