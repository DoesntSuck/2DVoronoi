using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEngine
{
    public static class PhysicsExtension
    {
        /// <summary>
        /// A linecast test to check if the given line intersects this collider. Collider is assigned to a temporary layer so the linecase operation
        /// targets ONLY this collider. An error will be thrown if another object thgat is not this collider is contacted
        /// </summary>
        public static bool Linecast(this Collider2D collider, Vector2 start, Vector2 end, out RaycastHit2D hitInfo)
        {
            // Original collider later
            var oriLayer = collider.gameObject.layer;

            // Temporary layer containing ONLY the given collider
            const int tempLayer = 31;
            collider.gameObject.layer = tempLayer;

            // Linecast against given collider (because its the only one on the temporary layer)
            hitInfo = Physics2D.Linecast(start, end, 1 << tempLayer);

            // Reset collider layer
            collider.gameObject.layer = oriLayer;

            // Check if something unexpected has been hit
            if (hitInfo.collider && hitInfo.collider != collider)
                throw new InvalidOperationException("Collider2D.Raycast() need a unique temp layer to work! Make sure Layer #" + tempLayer + " is unused!");

            // Whether or not the given collider was hit
            return hitInfo.collider != null;
        }
    }
}
