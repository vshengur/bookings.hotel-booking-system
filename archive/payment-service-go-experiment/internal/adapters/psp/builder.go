package psp

type Payload struct {
	AmountMinor int64  `json:"amountMinor"`
	Currency    string `json:"currency"`
	Customer    string `json:"customer"`
	Reference   string `json:"reference"`
}

type PayloadBuilder struct {
	p Payload
}

func NewPayloadBuilder() *PayloadBuilder { return &PayloadBuilder{} }

func (b *PayloadBuilder) WithAmount(amount int64, currency string) *PayloadBuilder {
	b.p.AmountMinor = amount; b.p.Currency = currency; return b
}

func (b *PayloadBuilder) WithCustomer(email string) *PayloadBuilder {
	b.p.Customer = email; return b
}

func (b *PayloadBuilder) WithReference(ref string) *PayloadBuilder {
	b.p.Reference = ref; return b
}

func (b *PayloadBuilder) Build() Payload { return b.p }
