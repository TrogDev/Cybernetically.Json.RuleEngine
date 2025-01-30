using Cybernetically.Json.RuleEngine.Enums;
using Cybernetically.Json.RuleEngine.Sctructures;
using Newtonsoft.Json.Linq;

namespace Cybernetically.Json.RuleEngine;

public class Program
{
    public static void Main(string[] args)
    {
        JsonRuleEngine engine = new JsonRuleEngine();

        engine.AddRule(
            new Rule()
            {
                Query = [new Sensor() { Search = ["language_tag"] }],
                Value = "$remove",
                ValueType = RuleValueType.Command,
                Type = RuleType.Recursive
            }
        );
        engine.AddRule(
            new Rule()
            {
                Query = [new Sensor() { Search = ["marketplace_id"] }],
                Value = "$remove",
                ValueType = RuleValueType.Command,
                Type = RuleType.Recursive
            }
        );
        engine.AddRule(
            new Rule()
            {
                Query = [new Sensor() { Search = ["$length"], Value = "1" }],
                Value = "$removeStep",
                ValueType = RuleValueType.Command,
                Type = RuleType.Recursive
            }
        );
        engine.AddRule(
            new Rule()
            {
                Query =
                [
                    new Sensor() { Search = ["$keysLength"], Value = "1" },
                    new Sensor() { Search = ["$root"], IsNegative = true }
                ],
                Value = "$removeStep",
                ValueType = RuleValueType.Command,
                Type = RuleType.Recursive
            }
        );
        engine.AddRule(
            new Rule()
            {
                Query =
                [
                    new Sensor() { Search = ["$root", "bullet_point"] }
                ],
                Value = "Bullet points",
                ValueType = RuleValueType.Key
            }
        );

        JObject json = JObject.Parse(
            @"
                {
                    'color': [
                        {
                            'language_tag': 'en_US',
                            'value': 'Multi Color',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'skip_offer': [{ 'value': false, 'marketplace_id': 'ATVPDKIKX0DER' }],
                    'fulfillment_availability': [
                        {
                            'fulfillment_channel_code': 'DEFAULT',
                            'quantity': 100,
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'manufacturer': [
                        {
                            'language_tag': 'en_US',
                            'value': 'HandMade',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'item_weight': [
                        {
                            'unit': 'pounds',
                            'value': 1.0,
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'model_name': [
                        {
                            'language_tag': 'en_US',
                            'value': 'Candle 001',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'bullet_point': [
                        {
                            'language_tag': 'en_US',
                            'value': 'Long Burn Time: Enjoy hours of soothing candlelight to unwind after a busy day or enhance your self-care routine.',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        },
                        {
                            'language_tag': 'en_US',
                            'value': 'Elegant Design: Encased in a minimalist glass jar, it complements any d\u00E9cor and makes a perfect gift for loved ones or a treat for yourself.',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        },
                        {
                            'language_tag': 'en_US',
                            'value': 'Eco-Friendly Materials: Made with 100% natural soy wax and a lead-free cotton wick for a clean, long-lasting burn.',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        },
                        {
                            'language_tag': 'en_US',
                            'value': 'Hand-Poured Craftsmanship: Each candle is carefully hand-poured in small batches, ensuring quality and attention to detail in every piece.',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        },
                        {
                            'language_tag': 'en_US',
                            'value': 'Soothing Lavender Aroma: Infused with pure lavender essential oil, this candle fills your home with a calming and refreshing floral scent that promotes relaxation and stress relief.',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'scent': [
                        {
                            'language_tag': 'en_US',
                            'value': 'Fruit',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'model_number': [
                        { 'value': 'LAVCND-14OZ-001', 'marketplace_id': 'ATVPDKIKX0DER' }
                    ],
                    'product_description': [
                        {
                            'language_tag': 'en_US',
                            'value': 'Indulge in the calming essence of lavender with our Lavender Serenity Handmade Candle. Thoughtfully crafted to transform any space into a tranquil retreat, this candle combines natural ingredients with artisanal care for an unmatched sensory experience.',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'supplier_declared_dg_hz_regulation': [
                        { 'value': 'not_applicable', 'marketplace_id': 'ATVPDKIKX0DER' }
                    ],
                    'brand': [
                        {
                            'language_tag': 'en_US',
                            'value': 'Generic',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'supplier_declared_has_product_identifier_exemption': [
                        { 'value': true, 'marketplace_id': 'ATVPDKIKX0DER' }
                    ],
                    'country_of_origin': [{ 'value': 'US', 'marketplace_id': 'ATVPDKIKX0DER' }],
                    'merchant_shipping_group': [
                        {
                            'value': 'legacy-template-id',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'item_name': [
                        {
                            'language_tag': 'en_US',
                            'value': 'Cozy Glow Handmade Candle',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'list_price': [
                        {
                            'currency': 'USD',
                            'value': 100.0,
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'item_type_keyword': [
                        {
                            'value': 'aromatherapy-candles',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'merchant_suggested_asin': [
                        { 'value': 'B0DTZ27WNR', 'marketplace_id': 'ATVPDKIKX0DER' }
                    ],
                    'condition_type': [
                        { 'value': 'new_new', 'marketplace_id': 'ATVPDKIKX0DER' }
                    ],
                    'number_of_items': [{ 'value': 1, 'marketplace_id': 'ATVPDKIKX0DER' }],
                    'material': [
                        {
                            'language_tag': 'en_US',
                            'value': 'Beeswax',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'item_width_height': [
                        {
                            'height': { 'unit': 'inches', 'value': 3.5 },
                            'width': { 'unit': 'inches', 'value': 2.5 },
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'warranty_description': [
                        {
                            'language_tag': 'en_US',
                            'value': '30 days',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'part_number': [
                        { 'value': 'LAV-HMCND-001', 'marketplace_id': 'ATVPDKIKX0DER' }
                    ],
                    'specific_uses_for_product': [
                        {
                            'language_tag': 'en_US',
                            'value': 'Aromatherapy',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'unit_count': [
                        {
                            'type': { 'language_tag': 'en_US', 'value': 'Ounce' },
                            'value': 14.0,
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'batteries_included': [
                        { 'value': false, 'marketplace_id': 'ATVPDKIKX0DER' }
                    ],
                    'other_product_image_locator_4': [
                        {
                            'media_location': 'https://m.media-amazon.com/images/I/217P0\u002BfGP2L.jpg',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'other_product_image_locator_3': [
                        {
                            'media_location': 'https://m.media-amazon.com/images/I/31ExTCdrj3L.jpg',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'other_product_image_locator_2': [
                        {
                            'media_location': 'https://m.media-amazon.com/images/I/21upSqtXtwL.jpg',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'other_product_image_locator_1': [
                        {
                            'media_location': 'https://m.media-amazon.com/images/I/21ipkat-ATL.jpg',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'main_product_image_locator': [
                        {
                            'media_location': 'https://m.media-amazon.com/images/I/31Tge4aDmBL.jpg',
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ],
                    'purchasable_offer': [
                        {
                            'currency': 'USD',
                            'start_at': { 'value': '2025-01-28T12:10:23.747Z' },
                            'end_at': { 'value': null },
                            'audience': 'ALL',
                            'our_price': [{ 'schedule': [{ 'value_with_tax': 100.0 }] }],
                            'marketplace_id': 'ATVPDKIKX0DER'
                        }
                    ]
                }
        ".Replace("'", "\"")
        );

        foreach (JProperty property in json.Properties().Skip(6).Take(1))
        {
            JObject attribute = new JObject(property.DeepClone());
            ProcessResponse response = engine.Process(attribute);
            Console.WriteLine(response.Result.ToString());
            Console.WriteLine("______");

            JsonRuleEngine negativeEngine = new JsonRuleEngine();

            foreach (Rule negativeRule in response.NegativeRules)
            {
                negativeEngine.AddRule(negativeRule);
            }

            Console.WriteLine(negativeEngine.Process(response.Result).Result.ToString());
        }
    }
}
