using Platform.Common.Flux.Domain;
using Platform.Common.Platform.Rest;

namespace Platform.Common.Flux.Csv
{
/**
 * This class us used to construct FluxResult from CSV.
 *
 * @see org.influxdata.flux
 */
    public class FluxCsvParser
    {

        private static int ERROR_RECORD_INDEX = 4;

        private enum ParsingState
        {
            NORMAL,

            IN_ERROR
        }

        public interface IFluxResponseConsumer
        {
            /**
             * Add new {@link FluxTable} to consumer.
             *
             * @param index       index of table
             * @param cancellable cancellable
             * @param table       new {@link FluxTable}
             */
            void Accept(int index, ICancellable cancellable, FluxTable table);

            /**
             * Add new {@link FluxRecord} to consumer.
             *
             * @param index       index of table
             * @param cancellable cancellable
             * @param record      new {@link FluxRecord}
             */

            void Accept(int index, ICancellable cancellable, FluxRecord record);
        }
    }
}